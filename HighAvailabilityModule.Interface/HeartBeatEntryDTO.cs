// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Interface
{
    using System;
    public class HeartBeatEntryDTO
    {
        public HeartBeatEntryDTO(string uuid, string utype, string uname, HeartBeatEntry lastSeenEntry)
        {
            this.Uuid = uuid;
            this.Utype = utype;
            this.Uname = uname;
            this.LastSeenEntry = lastSeenEntry;
        }

        public string Uuid { get; }

        public string Utype { get; }

        public string Uname { get; }

        public HeartBeatEntry LastSeenEntry { get; }
    }
}
