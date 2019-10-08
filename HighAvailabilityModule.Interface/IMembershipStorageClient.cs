// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IMembershipStorageClient
    {
        TimeSpan OperationTimeout { get; set; }

        Task<(string value, string type)> GetDataEntryAsync(string path, string key);
        Task<Guid> TryGetGuidAsync(string path, string key);
        Task<string> TryGetStringAsync(string path, string key);
        Task<int> TryGetIntAsync(string path, string key);
        Task<long> TryGetLongAsync(string path, string key);
        Task<double> TryGetDoubleAsync(string path, string key);
        Task<string[]> TryGetStringArrayAsync(string path, string key);
        Task<byte[]> TryGetByteArrayAsync(string path, string key);

        Task SetGuidAsync(string path, string key, Guid value, bool forceWrite);
        Task SetStringAsync(string path, string key, string value, bool forceWrite);
        Task SetIntAsync(string path, string key, int value, bool forceWrite);
        Task SetLongAsync(string path, string key, long value, bool forceWrite);
        Task SetDoubleAsync(string path, string key, double value, bool forceWrite);
        Task SetStringArrayAsync(string path, string key, string[] value, bool forceWrite);
        Task SetByteArrayAsync(string path, string key, byte[] value, bool forceWrite);

        Task DeleteDataEntryAsync(string path, string key);

        Task<List<string>> EnumerateDataEntryAsync(string path);

        Task Monitor(string path, string key, TimeSpan interval, Action<string, string> callback);
    }
}
