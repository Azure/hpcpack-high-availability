// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.E2ETest.TestCases.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using HighAvailabilityModule.Algorithm;
    using HighAvailabilityModule.Interface;

    public class AlgorithmContainer
    {
        private readonly string utype;

        private readonly string uname;

        private readonly TimeSpan interval;

        private readonly TimeSpan timeout;

        private readonly Func<string, string, TimeSpan, IMembershipClient> clientFactory;

        public MembershipWithWitness Algo { get; private set; }

        public AlgorithmContainer(string utype, string uname, TimeSpan interval, TimeSpan timeout, Func<string, string, TimeSpan, IMembershipClient> clientFactory)
        {
            this.utype = utype;
            this.uname = uname;
            this.interval = interval;
            this.timeout = timeout;
            this.clientFactory = clientFactory;
        }

        public void BuildAlgoInstance()
        {
            TestClient client = new TestClient(this.clientFactory(this.utype, this.uname, this.interval));
            MembershipWithWitness algo = new MembershipWithWitness(client, this.interval, this.timeout);
#pragma warning disable 4014
            algo.RunAsync(null, null);
#pragma warning restore 4014
            this.Algo = algo;
        }

        public async Task CrashRestart()
        {
            Console.WriteLine($"Fail instance {this.Algo.Dump()} ");
            Trace.TraceInformation($"[{DateTime.UtcNow:O}] Fail instance {this.Algo.Dump()}");
            this.Algo.Stop();
            await Task.Delay(1000);
            this.BuildAlgoInstance();
        }
    }
}