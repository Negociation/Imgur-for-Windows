using Newtonsoft.Json;
using System.Collections.Generic;

namespace Imgur.Api.Services.Models.Response.Notification
{
    public class AccountNotificationsData
    {
        [JsonProperty("messages")]
        public List<NotificationResponse> Messages { get; set; }

        [JsonProperty("replies")]
        public List<NotificationResponse> Replies { get; set; }
    }
}
