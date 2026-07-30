using Newtonsoft.Json;

namespace Imgur.Api.Services.Models.Response.Notification
{
    /// <summary>
    /// Conteúdo de notificação: reply (comentário) ou mensagem privada.
    /// </summary>
    public class NotificationContentResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
