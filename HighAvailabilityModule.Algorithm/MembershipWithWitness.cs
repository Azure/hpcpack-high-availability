// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Algorithm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class MembershipWithWitness
    {
        public string Uuid { get; }

        public string Utype { get; }

        public string Uname { get; }

        private IMembershipClient Client { get; }

        private TimeSpan HeartBeatInterval { get; }

        private TimeSpan HeartBeatTimeout { get; }

        private double WarningLatencyMS => this.HeartBeatInterval.TotalMilliseconds / 10;

        private readonly object heartbeatLock = new object();

        private CancellationTokenSource AlgorithmCancellationTokenSource { get; } = new CancellationTokenSource();

        internal CancellationToken AlgorithmCancellationToken => this.AlgorithmCancellationTokenSource.Token;

        public static readonly TraceSource Ts = new TraceSource("Microsoft.Hpc.HighAvailablity.Algorithm");

        private string affinityType = string.Empty;

        // Performance Statistics Section
        private double ewa5 = 0;

        private double maxLatency = 0;

        private double maxEwa5 = 0;

        private const double Weight5 = 0.2;

        private double ewa100 = 0;

        private double maxEwa100 = 0;

        private const double Weight100 = 0.01;

        private int totalFailedHeartBeat = 0;

        private int consecutiveFailedHeartBeat = 0;

        private int maxConsecutiveFailedHeartBeat = 0;

        private string AffinityType
        {
            get => this.affinityType;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.affinityType = string.Empty;
                    Ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}]Affinity Disabled");
                }
                else
                {
                    this.affinityType = value;
                    Ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}]Affinity Set to {value}");
                }
            }
        }

        private const string LastSeenHeartBeatString = "LastSeenHeartBeat";

        private const string LastSeenAffinityString = "LastSeenAffinity";

        private readonly Dictionary<string, (HeartBeatEntry Entry, DateTime QueryTime)> lastSeenHeartBeatDict =
            new Dictionary<string, (HeartBeatEntry Entry, DateTime QueryTime)> { { LastSeenHeartBeatString, (null, default(DateTime)) }, { LastSeenAffinityString, (null, default(DateTime)) } };

        public MembershipWithWitness(IMembershipClient client, TimeSpan heartBeatInterval, TimeSpan heartBeatTimeout, string affinityType)
        {
            this.Client = client;
            this.Client.OperationTimeout = heartBeatInterval;
            this.Uuid = client.GenerateUuid();
            this.Utype = client.Utype;
            this.Uname = client.Uname;
            this.HeartBeatInterval = heartBeatInterval;
            this.HeartBeatTimeout = heartBeatTimeout;
            this.AffinityType = affinityType;
        }

        public async Task RunAsync(Func<Task> onStartAsync, Func<Task> onErrorAsync)
        {
            try
            {
                await this.GetPrimaryAsync().ConfigureAwait(false);
                if (onStartAsync != null)
                {
                    ThreadPool.QueueUserWorkItem(_ => onStartAsync().GetAwaiter().GetResult());
                }

                await this.KeepPrimaryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Ts.TraceEvent(TraceEventType.Error, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}]Exception happen in RunAsync:{Environment.NewLine} {ex.ToString()}");
                throw;
            }
            finally
            {
                if (onErrorAsync != null)
                {
                    await onErrorAsync().ConfigureAwait(false);
                }
            }
        }

        public void Stop()
        {
            this.AlgorithmCancellationTokenSource.Cancel();
            Ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Algorithm stopped");
        }

        internal async Task GetPrimaryAsync()
        {
            var token = this.AlgorithmCancellationToken;
            while (!this.RunningAsPrimary(DateTime.UtcNow) || !AffinityAsPrimary(DateTime.UtcNow))
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(this.HeartBeatInterval, token).ConfigureAwait(false);
                await this.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
                await this.CheckAffinityAsync(DateTime.UtcNow).ConfigureAwait(false);

                if (!this.PrimaryUp && this.AffinityAsPrimary(DateTime.UtcNow))
                {
                    Ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Primary down");
                    await this.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
                }
            }
        }

        internal async Task CheckPrimaryAsync(DateTime now)
        {
            await this.CheckPrimaryAuxAsync(now, this.Utype, LastSeenHeartBeatString).ConfigureAwait(false);
        }

        private async Task CheckPrimaryAuxAsync(DateTime now, string qtype, string name)
        {
            try
            {
                var entry = await this.Client.GetHeartBeatEntryAsync(qtype).ConfigureAwait(false);
                if (now > this.lastSeenHeartBeatDict[name].QueryTime)
                {
                    lock (this.heartbeatLock)
                    {
                        if (now > this.lastSeenHeartBeatDict[name].QueryTime)
                        {
                            this.lastSeenHeartBeatDict[name] = (entry, now);
                        }
                    }
                }

                Ts.TraceEvent(
                    TraceEventType.Verbose,
                    0,
                    $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] {name} = {this.lastSeenHeartBeatDict[name].Entry.Uuid}, Client Type: {qtype}, {this.lastSeenHeartBeatDict[name].Entry.TimeStamp:O}");
            }
            catch (Exception ex)
            {
                Ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] {name} Error occured when getting heartbeat entry: {ex.ToString()}, Client Type: {qtype}");
            }
        }

        internal async Task HeartBeatAsPrimaryAsync()
        {
            if (this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry == null)
            {
                Ts.TraceEvent(TraceEventType.Warning, 0, $"[Protocol][{this.Uuid}] Can't send heartbeat before querying current primary.");
                throw new InvalidOperationException($"[Protocol][{this.Uuid}] Can't send heartbeat before querying current primary.");
            }

            var sendTime = DateTime.UtcNow;
            try
            {
                Ts.TraceEvent(
                    TraceEventType.Verbose,
                    0,
                    $"[{sendTime:O}][Protocol][{this.Uuid}] Sending heartbeat with UUID = {this.Uuid} at localtime {sendTime:O}, lastSeenHeartBeat = {this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.Uuid}, {this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.TimeStamp:O}, Client Type: {this.Utype}");
                await this.Client.HeartBeatAsync(new HeartBeatEntryDTO(this.Uuid, this.Utype, this.Uname, this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry)).ConfigureAwait(false);
                DateTime completedTime = DateTime.UtcNow;
                this.consecutiveFailedHeartBeat = 0;
                double latency = (completedTime - sendTime).TotalMilliseconds;
                Ts.TraceEvent(
                    TraceEventType.Verbose,
                    0,
                    $"[{completedTime:O}][Protocol][{this.Uuid}] Sending heartbeat with UUID = {this.Uuid} at localtime {sendTime:O} completed, Client Type: {this.Utype}, latency: {latency:F1}/{this.WarningLatencyMS:F1} ms");
                
                this.maxLatency = Math.Max(latency, this.maxLatency);

                this.ewa5 = (Weight5 * latency) + ((1 - Weight5) * this.ewa5);
                this.maxEwa5 = Math.Max(this.ewa5, this.maxEwa5);

                this.ewa100 = (Weight100 * latency) + ((1 - Weight100) * this.ewa100);
                this.maxEwa100 = Math.Max(this.ewa100, this.maxEwa100);
              
                if (latency > this.WarningLatencyMS)
                {
                    Ts.TraceEvent(
                        TraceEventType.Warning,
                        0,
                        $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Sending heartbeat with UUID = {this.Uuid} at localtime {sendTime:O} reached warning latency level {latency:F1}/{this.WarningLatencyMS:F1} ms, Client Type: {this.Utype}");

                    Ts.TraceEvent(TraceEventType.Warning, 0, this.GetPerfString(latency));
                }
                else
                {
                    Ts.TraceEvent(TraceEventType.Verbose, 0, this.GetPerfString(latency));
                }
            }
            catch (Exception ex)
            {
                Ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Error occured when updating heartbeat entry at localtime {sendTime:O}: {ex.ToString()}");
                Ts.TraceEvent(TraceEventType.Warning, 0, this.GetPerfString(double.NaN));

                Interlocked.Increment(ref this.totalFailedHeartBeat);
                Interlocked.Increment(ref this.consecutiveFailedHeartBeat);
                this.maxConsecutiveFailedHeartBeat = Math.Max(this.consecutiveFailedHeartBeat, this.maxConsecutiveFailedHeartBeat);
            }
        }

        private string GetPerfString(double latency) =>
            $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}](Performance) Latency(C/M/W/T): {latency:F1} / {this.maxLatency:F1} / {this.WarningLatencyMS:F1} / {this.HeartBeatTimeout.TotalMilliseconds:F1}, EWA(5/100/M5/M100): {this.ewa5:F1} / {this.ewa100:F1} / {this.maxEwa5:F1} / {this.maxEwa100:F1}, Failed HB(C/M/T): {this.consecutiveFailedHeartBeat} / {this.maxConsecutiveFailedHeartBeat} / {this.totalFailedHeartBeat}";


        /// <summary>
        /// Checks if current process is primary process
        /// </summary>
        internal async Task KeepPrimaryAsync()
        {
            var token = this.AlgorithmCancellationToken;
            while (this.RunningAsPrimary(DateTime.UtcNow) && this.AffinityAsPrimary(DateTime.UtcNow))
            {
                token.ThrowIfCancellationRequested();
#pragma warning disable 4014
                this.HeartBeatAsPrimaryAsync();
                this.CheckPrimaryAsync(DateTime.UtcNow);
                this.CheckAffinityAsync(DateTime.UtcNow);
#pragma warning restore 4014
                await Task.Delay(this.HeartBeatInterval, token).ConfigureAwait(false);
            }
            Ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Lost Primary");
        }

        private bool PrimaryUp => this.lastSeenHeartBeatDict[LastSeenHeartBeatString] != default && !this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.IsEmpty;

        private bool AffinityPrimaryUp => this.lastSeenHeartBeatDict[LastSeenAffinityString] != default && !this.lastSeenHeartBeatDict[LastSeenAffinityString].Entry.IsEmpty;

        internal bool RunningAsPrimary(DateTime now)
        {
            var primary = this.PrimaryUp
                          && this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.Uuid == this.Uuid
                          && now - this.lastSeenHeartBeatDict[LastSeenHeartBeatString].QueryTime < (this.HeartBeatTimeout - this.HeartBeatInterval);
            if (!primary)
            {
                Ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol] Running as secondary {this.Dump()}.");
            }

            return primary;
        }

        // TODO: consolidate with RunningAsPrimary
        internal bool AffinityAsPrimary(DateTime now)
        {
            var affinityPrimary = this.AffinityType == string.Empty
                                  || (this.AffinityPrimaryUp
                                      && this.lastSeenHeartBeatDict[LastSeenAffinityString].Entry.Uname.ToLower() == this.Uname.ToLower()
                                      && now - this.lastSeenHeartBeatDict[LastSeenAffinityString].QueryTime < (this.HeartBeatTimeout - this.HeartBeatInterval));
            if (!affinityPrimary)
            {
                Ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol] Affinity service is running on another machine. {this.Dump()}.");
            }

            return affinityPrimary;
        }

        internal async Task CheckAffinityAsync(DateTime now)
        {
            await this.CheckPrimaryAuxAsync(now, this.AffinityType, LastSeenAffinityString).ConfigureAwait(false);
        }

        public string Dump() =>
            $"PrimaryUp = {this.PrimaryUp}, SelfUuid = {this.Uuid ?? string.Empty}, LastSeenUuid = {this.lastSeenHeartBeatDict[LastSeenHeartBeatString].Entry?.Uuid ?? string.Empty}, LastSeenQueryTime = {this.lastSeenHeartBeatDict[LastSeenHeartBeatString].QueryTime:O}, Client Type: {this.Utype}";
    }
}