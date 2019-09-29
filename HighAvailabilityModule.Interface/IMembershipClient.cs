// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Interface
{
    using System;

    public interface IMembershipClient : IMembership
    {
        string GenerateUuid();

        string Utype { get; }

        string Uname { get; }

        TimeSpan OperationTimeout { get; set; }
    }
}
