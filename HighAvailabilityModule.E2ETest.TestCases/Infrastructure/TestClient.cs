// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.E2ETest.TestCases.Infrastructure
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class TestClient : IMembershipClient
    {
        public TestClient(IMembershipClient impl, NetworkConfiguration net)
        {
            this.membershipClientImplementation = impl;
            this.net = net;
        }

        public TestClient(IMembershipClient impl)
        {
            this.membershipClientImplementation = impl;
            this.net = NetworkConfiguration.Reliable;
        }

        private readonly IMembershipClient membershipClientImplementation;

        private readonly NetworkConfiguration net;

        private bool MessageLost => new Random().NextDouble() < this.net.MessageLostRate;

        private async Task LoseMessage()
        {
            if (this.MessageLost)
            {
                Trace.TraceInformation($"Message Lost: {this.membershipClientImplementation.Utype} - {this.membershipClientImplementation.Uname}");
                Console.WriteLine($"Message Lost: {this.membershipClientImplementation.Utype} - {this.membershipClientImplementation.Uname}");

                await Task.Delay(this.membershipClientImplementation.OperationTimeout).ConfigureAwait(false);
                throw new TimeoutException();
            }
        }

        public async Task HeartBeatAsync(HeartBeatEntryDTO entryDTO)
        {
            await this.LoseMessage().ConfigureAwait(false);
            await this.membershipClientImplementation.HeartBeatAsync(entryDTO).ConfigureAwait(false);
        }

        public async Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype)
        {
            await this.LoseMessage().ConfigureAwait(false);
            var res = await this.membershipClientImplementation.GetHeartBeatEntryAsync(utype).ConfigureAwait(false);
            await this.LoseMessage().ConfigureAwait(false);
            return res;
        }

        public string GenerateUuid() => this.membershipClientImplementation.GenerateUuid();

        public string Utype => this.membershipClientImplementation.Utype;

        public string Uname => this.membershipClientImplementation.Uname;

        public TimeSpan OperationTimeout
        {
            get => this.membershipClientImplementation.OperationTimeout;
            set => this.membershipClientImplementation.OperationTimeout = value;
        }
    }
}
