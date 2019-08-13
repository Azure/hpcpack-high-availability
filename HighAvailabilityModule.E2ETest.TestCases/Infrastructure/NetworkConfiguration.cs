// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.E2ETest.TestCases.Infrastructure
{
    public class NetworkConfiguration
    {
        public long LatencyLowerBound { get; set; } = 0;

        public long LatencyUpperBound { get; set; } = 0;

        public double MessageLostRate { get; set; } = 0;

        public static readonly NetworkConfiguration Reliable = new NetworkConfiguration();
    }
}
