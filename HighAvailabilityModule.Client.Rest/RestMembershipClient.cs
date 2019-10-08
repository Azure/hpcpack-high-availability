// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Client.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class RestMembershipClient : IMembershipClient
    {
        private readonly RestClientImpl impl;

        private readonly HttpClient httpClient;

        public string Uuid { get; }

        public string Utype { get; }

        public string Uname { get; }

        public RestMembershipClient(string utype, string uname, TimeSpan operationTimeout)
        {
            this.httpClient = new HttpClient { Timeout = operationTimeout };
            this.impl = new RestClientImpl(this.httpClient);
            this.Uuid = Guid.NewGuid().ToString();
            this.Utype = utype;
            this.Uname = uname;
        }

        public RestMembershipClient()
        {
            this.httpClient = new HttpClient();
            this.impl = new RestClientImpl(this.httpClient);
        }

        public static RestMembershipClient CreateNew(string utype, string uname, TimeSpan operationTimeout) => new RestMembershipClient(utype, uname, operationTimeout);

        public string BaseUri
        {
            get => this.impl.BaseUrl;
            set => this.impl.BaseUrl = value;
        }

        public Task HeartBeatAsync(HeartBeatEntryDTO entryDTO)
        {
            if (this.Uuid == default || string.IsNullOrEmpty(this.Utype) || string.IsNullOrEmpty(this.Uname))
            {
                throw new InvalidOperationException("Can't sent heartbeat from a read only client.");
            }

            return this.impl.HeartBeatAsync(entryDTO);
        }

        public Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype) => this.impl.GetHeartBeatEntryAsync(utype);

        public string GenerateUuid() => this.Uuid;

        public TimeSpan OperationTimeout
        {
            get => this.httpClient.Timeout;
            set => this.httpClient.Timeout = value;
        }
    }
}