using Newtonsoft.Json;

namespace Imgur.Api.Services.Models.Response.Image
{
    public class UploadResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("deletehash")]
        public string DeleteHash { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}