// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.Server.Rest.Controllers
{
    using System.Threading.Tasks;

    using HighAvailabilityModule.Interface;

    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase, IMembership
    {
        private readonly IMembership membershipImpl;

        public MembershipController(IMembership membershipImplementation)
        {
            this.membershipImpl = membershipImplementation;
        }

        [HttpGet("ping")]
        public async Task<bool> Ping()
        {
            return true;
        }

        [HttpPost("heartbeat")]
        public async Task HeartBeatAsync([FromBody] HeartBeatEntryDTO entryDTO)
        {
            await this.membershipImpl.HeartBeatAsync(entryDTO);
        }

        [HttpGet("heartbeat/{utype}")]
        public async Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype) => await this.membershipImpl.GetHeartBeatEntryAsync(utype);
    }
}