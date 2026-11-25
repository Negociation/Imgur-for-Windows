using System.Collections.Generic;

namespace Imgur.Api.Services.Models.Response.Account
{
    /// <summary>GET account/me/available_avatars — shape varies; we accept a flat name list when present.</summary>
    public class AvailableAvatarsResponse
    {
        public List<string> available_avatars { get; set; }
        public int available_avatars_count { get; set; }
    }
}
