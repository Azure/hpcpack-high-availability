// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.E2ETest.TestCases
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.E2ETest.TestCases.Infrastructure;
    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class ChaosTest
    {
        private readonly Func<string, string, TimeSpan, IMembershipClient> clientFactory;

        private readonly IMembershipClient judge;

        private readonly NetworkConfiguration netconf = new NetworkConfiguration { MessageLostRate = 0.1 };

        public ChaosTest(Func<string, string, TimeSpan, IMembershipClient> clientFactory, IMembershipClient judge)
        {
            this.clientFactory = clientFactory;
            this.judge = judge;
        }

        public async Task Start(string utype)
        {
            AlgorithmController controller = new AlgorithmController(
                2,
                utype,
                TimeSpan.FromSeconds(0.2),
                TimeSpan.FromSeconds(1),
                (type, name, timeout) => new TestClient(this.clientFactory(type, name, timeout), this.netconf),
                this.judge);
            Task.Run(controller.Start);

            while (true)
            {
                try
                {
                    await controller.WatchResult();
                }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceError($"Liveness validation detected! {ex.ToString()}");
                    Console.WriteLine($"Liveness validation detected! {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Exception happened in chaos test: {ex.ToString()}");
                    Console.WriteLine($"Exception happened in chaos test: {ex.ToString()}");
                }
            }
        }
    }
}