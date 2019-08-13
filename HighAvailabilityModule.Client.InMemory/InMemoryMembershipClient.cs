// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.Client.InMemory
{
    using System;
    using System.Threading.Tasks;

    using HighAvailabilityModule.Interface;

    public class InMemoryMembershipClient : IMembershipClient
    {
        public InMemoryMembershipClient(IMembership membershipServer, string uuid, string utype, string uname)
        {
            this.serverImplementation = membershipServer;
            this.Uuid = uuid;
            this.Utype = utype;
            this.Uname = uname;
        }

        private readonly IMembership serverImplementation;

        public string Uuid { get; }

        public string Utype { get; set; }

        public string Uname { get; set; }

        public Task HeartBeatAsync(HeartBeatEntryDTO entryDTO) => this.serverImplementation.HeartBeatAsync(entryDTO);

        public Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype) => this.serverImplementation.GetHeartBeatEntryAsync(utype);

        public string GenerateUuid() => this.Uuid;

        public TimeSpan OperationTimeout { get; set; }
    }
}