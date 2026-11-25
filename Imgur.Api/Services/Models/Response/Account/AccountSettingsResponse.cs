namespace Imgur.Api.Services.Models.Response.Account
{
    /// <summary>Subset of GET account/me/settings used by future Edit Profile / privacy UI.</summary>
    public class AccountSettingsResponse
    {
        public string account_url { get; set; }
        public string email { get; set; }
        public string bio { get; set; }
        public bool public_images { get; set; }
        public bool messaging_enabled { get; set; }
        public string album_privacy { get; set; }
        public bool show_mature { get; set; }
        public bool newsletter_subscribed { get; set; }
    }
}
