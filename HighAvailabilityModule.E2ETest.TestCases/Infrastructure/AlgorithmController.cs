// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.E2ETest.TestCases.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using HighAvailabilityModule.Interface;

    public class AlgorithmController
    {
        public int Count { get; }

        public string Utype { get; }

        public TimeSpan Interval { get; }

        public TimeSpan Timeout { get; }

        public Func<string, string, TimeSpan, IMembershipClient> ClientFactory { get; }

        private readonly AlgorithmContainer[] containers;

        private readonly IMembershipClient judge;

        private HashSet<string> GetLivingClientIds() => this.containers.Select(c => c.Algo.Uuid).ToHashSet();

        private (HeartBeatEntry entry, DateTime queryTime)? lastError = null;

        public AlgorithmController(int count, string utype, TimeSpan interval, TimeSpan timeout, Func<string, string, TimeSpan, IMembershipClient> clientFactory, IMembershipClient judge)
        {
            this.Count = count;
            this.Utype = utype;
            this.Interval = interval;
            this.Timeout = timeout;
            this.ClientFactory = clientFactory;
            this.containers = new AlgorithmContainer[this.Count];
            this.judge = judge;
        }

        public void Start()
        {
            for (int i = 0; i != this.Count; ++i)
            {
                this.containers[i] = new AlgorithmContainer(this.Utype, i.ToString(), this.Interval, this.Timeout, this.ClientFactory);
                this.containers[i].BuildAlgoInstance();
            }

            Task.Run(this.FailProcess);
        }

        public void CheckLiveness(HeartBeatEntry entry)
        {
            if (lastError != null)
            {
                Trace.TraceInformation($"Client Type: {this.Utype}    Entry Uuid:    {entry.Uuid}    lastError.Uuid: {this.lastError.Value.entry.Uuid}");
            }
            else
            {
                Trace.TraceInformation($"Client Type: {this.Utype}    Entry Uuid:    {entry.Uuid}    lastError.Uuid: null");
            }
            if (!this.GetLivingClientIds().Contains(entry.Uuid))
            {
                if (this.lastError == null || this.lastError.Value.entry.Uuid != entry.Uuid)
                {
                    this.lastError = (entry, DateTime.UtcNow);
                }
                else if (DateTime.UtcNow - this.lastError.Value.queryTime > (2 * this.Timeout)) 
                {
                    string ExceptionID = Guid.NewGuid().ToString();
                    Trace.TraceError($"Client Type{this.Utype}    [{ExceptionID}]Liveness violation detected.");
                    Trace.TraceInformation($"UtcNow: {DateTime.UtcNow:O}    lastError.queryTime: {this.lastError.Value.queryTime:O}");
                    throw new InvalidOperationException($"[{ExceptionID}]Liveness violation detected.");
                }
            }
            else
            {
                this.lastError = null;
            }
        }

        public async Task FailProcess()
        {
            while (true)
            {
                Random rand = new Random();
                int tofail = rand.Next(0, this.Count);
                if (tofail != 0)

                {
                    try
                    {
                        await Task.WhenAll(this.containers.OrderBy(_ => Guid.NewGuid()).Take(tofail).Select(c => c.CrashRestart()));
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

                await Task.Delay(this.Interval * 2);
            }
        }

        public async Task WatchResult()
        {
            while (true)
            {
                try
                {
                    var entry = await this.judge.GetHeartBeatEntryAsync(this.Utype);
                    var livingClients = this.GetLivingClientIds();
                    Trace.TraceInformation($"Healthy:{livingClients.Contains(entry.Uuid)}, livingClients: {string.Join(",", livingClients)} Client Type: {this.Utype}");
                    Console.WriteLine($"Healthy:{livingClients.Contains(entry.Uuid)}, livingClients: {string.Join(",", livingClients)} Client Type: {this.Utype}");

                    this.CheckLiveness(entry);
                }
                
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.ToString());
                    Console.WriteLine(ex.ToString());
                    throw;
                }
                finally
                {
                    await Task.Delay(this.Interval * 2).ConfigureAwait(false);
                }
            }
        }
    }
}