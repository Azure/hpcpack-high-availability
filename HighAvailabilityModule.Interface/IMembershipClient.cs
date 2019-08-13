// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.Interface
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
