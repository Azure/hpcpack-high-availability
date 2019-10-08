// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.E2ETest.TestCases
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.E2ETest.TestCases.Infrastructure;
    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class BasicTest
    {
        private readonly Func<string, string, TimeSpan, IMembershipClient> clientFactory;

        private readonly IMembershipClient judge;

        public BasicTest(Func<string, string, TimeSpan, IMembershipClient> clientFactory, IMembershipClient judge)
        {
            this.clientFactory = clientFactory;
            this.judge = judge;
        }

        public async Task Start(string type)
        {
            AlgorithmController controller = new AlgorithmController(2, type, TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(1), this.clientFactory, this.judge);
            Task.Run(controller.Start);
            await Task.Run(controller.WatchResult);
        }

        private async Task ShowLeader()
        {
            while (true)
            {
                Console.WriteLine(await this.judge.GetHeartBeatEntryAsync("A"));
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
