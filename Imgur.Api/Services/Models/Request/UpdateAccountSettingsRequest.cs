namespace Imgur.Api.Services.Models.Request
{
    /// <summary>Writable fields for PUT account/me/settings (all optional).</summary>
    public class UpdateAccountSettingsRequest
    {
        public string bio { get; set; }
        public string username { get; set; }
        /// <summary>API avatar name, e.g. flavor/taco or default/A.</summary>
        public string avatar { get; set; }
        public string public_images { get; set; }
        public string messaging_enabled { get; set; }
        public string album_privacy { get; set; }
        public string show_mature { get; set; }
        public string newsletter_subscribed { get; set; }
        public string accepted_gallery_terms { get; set; }
    }
}
