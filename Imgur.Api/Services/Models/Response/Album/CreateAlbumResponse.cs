using System;
using System.Collections.Generic;
using System.Text;

// AlbumResponse.cs
using Newtonsoft.Json;

namespace Imgur.Api.Services.Models.Response.Album
{
    public class CreateAlbumResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("deletehash")]
        public string DeleteHash { get; set; }
    }
}