using Newtonsoft.Json;

namespace Imgur.Api.Services.Models.Response.Notification
{
    public class NotificationResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("viewed")]
        public bool Viewed { get; set; }

        [JsonProperty("content")]
        public NotificationContentResponse Content { get; set; }
    }
}
