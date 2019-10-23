// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Server.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;

    public class InMemoryMembershipServer : IMembership
    {
        private HeartBeatEntry Current;

        public Dictionary<string, HeartBeatEntry> CurrentTable { get; set; } = new Dictionary<string, HeartBeatEntry>() {};

        private TimeSpan Timeout { get; }

        public TimeSpan ReplyDelay { get; set; } = TimeSpan.Zero;

        private object heartbeatLock = new object();

        public InMemoryMembershipServer(TimeSpan timeout)
        {
            this.Timeout = timeout;
        }

        public void RemoveCurrent()
        {
            lock (heartbeatLock)
            {
                List<string> key = new List<string>(this.CurrentTable.Keys);
                for (int i = 0; i < key.Count; i++)
                {
                    this.CurrentTable[key[i]] = HeartBeatEntry.Empty;
                }
            }
        }

        public Task HeartBeatAsync(HeartBeatEntryDTO entryDTO) => this.HeartBeatAsync(entryDTO, DateTime.UtcNow);

        public async Task HeartBeatAsync(HeartBeatEntryDTO entryDTO, DateTime now)
        {
            Guid operationGuid = Guid.NewGuid();
            Trace.TraceInformation($"[{now:O}][Server][{operationGuid}] Received Heart beat from {entryDTO.Uuid}, type {entryDTO.Utype}, machine {entryDTO.Uname}");
            bool ValidInput()
            {
                var valid = !this.CurrentTable.ContainsKey(entryDTO.Utype) || this.CurrentTable[entryDTO.Utype] == null
                        || (this.HeartbeatInvalid(entryDTO.Utype, now) && entryDTO.LastSeenEntry != null && entryDTO.LastSeenEntry.IsEmpty)
                        || (this.LastSeenEntryValid(entryDTO.Utype, entryDTO.LastSeenEntry) && this.CurrentTable[entryDTO.Utype].Uuid == entryDTO.Uuid && this.CurrentTable[entryDTO.Utype].Utype == entryDTO.Utype);
                if (!valid)
                {
                    Trace.TraceInformation($"[{now:O}][Server][{operationGuid}] Heart beat invalid.");
                }

                return valid;
            }

            if (!ValidInput())
            {
                return;
            }

            lock (this.heartbeatLock)
            {
                if (!ValidInput())
                {
                    return;
                }

                this.Current = new HeartBeatEntry(entryDTO.Uuid, entryDTO.Utype, entryDTO.Uname, now);

                this.CurrentTable[entryDTO.Utype] = this.Current;
                Trace.TraceInformation($"[{now:O}][Server][{operationGuid}] Current leader set to {entryDTO.Uuid}");
            }

            await Task.Delay(this.ReplyDelay).ConfigureAwait(false);
        }

        public Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype) => this.GetHeartBeatEntryAsync(utype, DateTime.UtcNow);

        public async Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype, DateTime now)
        {
            await Task.Delay(this.ReplyDelay).ConfigureAwait(false);

            if (this.HeartbeatInvalid(utype, now))
            {
                return HeartBeatEntry.Empty;
            }
            else
            {
                return this.CurrentTable[utype];
            }                
        }

        private bool HeartbeatInvalid(string utype, DateTime now)
        {
            return !this.CurrentTable.ContainsKey(utype) || this.CurrentTable[utype] == null || (now - this.CurrentTable[utype].TimeStamp >= this.Timeout);
        }

        private bool LastSeenEntryValid(string utype, HeartBeatEntry LastSeenEntry)
        {
            return this.CurrentTable.ContainsKey(utype) && LastSeenEntry != null && this.CurrentTable[utype].Uuid == LastSeenEntry.Uuid &&
            this.CurrentTable[utype].Utype == LastSeenEntry.Utype && this.CurrentTable[utype].TimeStamp == LastSeenEntry.TimeStamp;
        }
    }
}