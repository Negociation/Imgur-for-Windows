namespace Imgur.Models
{
    public enum AccountNotificationKind
    {
        Reply,
        Message
    }

    /// <summary>UI-facing Imgur account notification (reply or private message).</summary>
    public class AccountNotification
    {
        public long Id { get; set; }
        public bool Viewed { get; set; }
        public AccountNotificationKind Kind { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string ImageId { get; set; }

        public string AvatarUrl =>
            string.IsNullOrWhiteSpace(Author)
                ? null
                : "https://imgur.com/user/" + Author + "/avatar";
    }
}
