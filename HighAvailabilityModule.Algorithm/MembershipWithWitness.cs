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

        private readonly object heartbeatLock = new object();

        private CancellationTokenSource AlgorithmCancellationTokenSource { get; } = new CancellationTokenSource();

        internal CancellationToken AlgorithmCancellationToken => this.AlgorithmCancellationTokenSource.Token;

        public static readonly TraceSource ts = new TraceSource("Microsoft.Hpc.HighAvailablity.Algorithm");

        private string AffinityType;

        private const string LastSeenHeartBeatString = "LastSeenHeartBeat";

        private const string LastSeenAffinityString = "LastSeenAffinity";

        private Dictionary<string, (HeartBeatEntry Entry, DateTime QueryTime)> LastSeenHeartBeatDict 
            = new Dictionary<string, (HeartBeatEntry Entry, DateTime QueryTime)> { { LastSeenHeartBeatString, (null, default(DateTime))}, { LastSeenAffinityString, (null, default(DateTime)) } };

        public MembershipWithWitness(IMembershipClient client, TimeSpan heartBeatInterval, TimeSpan heartBeatTimeout, string AffinityType)
        {
            this.Client = client;
            this.Client.OperationTimeout = heartBeatInterval;
            this.Uuid = client.GenerateUuid();
            this.Utype = client.Utype;
            this.Uname = client.Uname;
            this.HeartBeatInterval = heartBeatInterval;
            this.HeartBeatTimeout = heartBeatTimeout;
            this.AffinityType = AffinityType;
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
                ts.TraceEvent(TraceEventType.Error, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}]Exception happen in RunAsync:{Environment.NewLine} {ex.ToString()}");
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
            ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Algorithm stopped");
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
                    ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Primary down");
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
                if (now > this.LastSeenHeartBeatDict[name].QueryTime)
                {
                    lock (this.heartbeatLock)
                    {
                        if (now > this.LastSeenHeartBeatDict[name].QueryTime)
                        {
                            this.LastSeenHeartBeatDict[name] = (entry, now);
                        }
                    }
                }
                ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] {name} = {this.LastSeenHeartBeatDict[name].Entry.Uuid}, Client Type: {qtype}, {this.LastSeenHeartBeatDict[name].Entry.TimeStamp:O}");
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] {name} Error occured when getting heartbeat entry: {ex.ToString()}, Client Type: {qtype}");
            }
        }

        internal async Task HeartBeatAsPrimaryAsync()
        {
            if (this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry == null)
            {
                ts.TraceEvent(TraceEventType.Warning, 0, $"[Protocol][{this.Uuid}] Can't send heartbeat before querying current primary.");
                throw new InvalidOperationException($"[Protocol][{this.Uuid}] Can't send heartbeat before querying current primary.");
            }

            try
            {
                var sendTime = DateTime.UtcNow;
                ts.TraceEvent(TraceEventType.Information, 0, $"[{sendTime:O}][Protocol][{this.Uuid}] Sending heartbeat with UUID = {this.Uuid} at localtime {sendTime:O}, lastSeenHeartBeat = {this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.Uuid}, {this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.TimeStamp:O}, Client Type: {this.Utype}");
                await this.Client.HeartBeatAsync(new HeartBeatEntryDTO (this.Uuid, this.Utype, this.Uname, this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry)).ConfigureAwait(false);
                ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Sending heartbeat with UUID = {this.Uuid} at localtime {sendTime:O} completed, Client Type: {this.Utype}");
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Error occured when updating heartbeat entry: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Checks if current process is primary process
        /// </summary>
        internal async Task KeepPrimaryAsync()
        {
            var token = this.AlgorithmCancellationToken;
            while (this.RunningAsPrimary(DateTime.UtcNow) && AffinityAsPrimary(DateTime.UtcNow))
            {
                token.ThrowIfCancellationRequested();
#pragma warning disable 4014
                this.HeartBeatAsPrimaryAsync();
                this.CheckPrimaryAsync(DateTime.UtcNow);
                this.CheckAffinityAsync(DateTime.UtcNow);
#pragma warning restore 4014
                await Task.Delay(this.HeartBeatInterval, token).ConfigureAwait(false);
            }
            ts.TraceEvent(TraceEventType.Warning, 0, $"[{DateTime.UtcNow:O}][Protocol][{this.Uuid}] Lost Primary");
        }

        private bool PrimaryUp => this.LastSeenHeartBeatDict[LastSeenHeartBeatString] != default && !this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.IsEmpty;

        private bool AffinityPrimaryUp => this.LastSeenHeartBeatDict[LastSeenAffinityString] != default && !this.LastSeenHeartBeatDict[LastSeenAffinityString].Entry.IsEmpty;

        internal bool RunningAsPrimary(DateTime now)
        {
            var primary = this.PrimaryUp && this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry.Uuid == this.Uuid && now - this.LastSeenHeartBeatDict[LastSeenHeartBeatString].QueryTime < (this.HeartBeatTimeout - this.HeartBeatInterval);
            if (!primary)
            {
                ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol] Running as secondary {this.Dump()}.");
            }
            return primary;
        }

        internal bool AffinityAsPrimary(DateTime now)
        {
            var AffinityPrimary = this.AffinityType == string.Empty || (this.AffinityPrimaryUp && this.LastSeenHeartBeatDict[LastSeenAffinityString].Entry.Uname.ToLower() == this.Uname.ToLower() && now - this.LastSeenHeartBeatDict[LastSeenAffinityString].QueryTime < (this.HeartBeatTimeout - this.HeartBeatInterval));
            if (!AffinityPrimary)
            {
                ts.TraceEvent(TraceEventType.Information, 0, $"[{DateTime.UtcNow:O}][Protocol] Affinity service is running on another machine. {this.Dump()}.");
            }
            return AffinityPrimary;
        }

        internal async Task CheckAffinityAsync(DateTime now)
        {
            await this.CheckPrimaryAuxAsync(now, this.AffinityType, LastSeenAffinityString).ConfigureAwait(false);
        }

        public string Dump() => $"PrimaryUp = {this.PrimaryUp}, SelfUuid = {this.Uuid ?? string.Empty}, LastSeenUuid = {this.LastSeenHeartBeatDict[LastSeenHeartBeatString].Entry?.Uuid ?? string.Empty}, LastSeenQueryTime = {this.LastSeenHeartBeatDict[LastSeenHeartBeatString].QueryTime:O}, Client Type: {this.Utype}";
    }
}