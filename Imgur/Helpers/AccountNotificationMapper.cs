using Imgur.Api.Services.Models.Response.Notification;
using Imgur.Models;
using System.Collections.Generic;
using System.Linq;

namespace Imgur.Helpers
{
    public static class AccountNotificationMapper
    {
        public static List<AccountNotification> Flatten(AccountNotificationsData data)
        {
            var list = new List<AccountNotification>();
            if (data == null)
                return list;

            if (data.Replies != null)
            {
                foreach (var n in data.Replies)
                {
                    var mapped = FromResponse(n, AccountNotificationKind.Reply);
                    if (mapped != null)
                        list.Add(mapped);
                }
            }

            if (data.Messages != null)
            {
                foreach (var n in data.Messages)
                {
                    var mapped = FromResponse(n, AccountNotificationKind.Message);
                    if (mapped != null)
                        list.Add(mapped);
                }
            }

            return list.OrderByDescending(n => n.Id).ToList();
        }

        public static AccountNotification FromResponse(NotificationResponse n, AccountNotificationKind? forcedKind = null)
        {
            if (n == null)
                return null;

            var content = n.Content;
            var looksLikeMessage = content != null
                && string.IsNullOrEmpty(content.Comment)
                && (!string.IsNullOrEmpty(content.From) || !string.IsNullOrEmpty(content.Subject) || !string.IsNullOrEmpty(content.Body));

            var kind = forcedKind ?? (looksLikeMessage ? AccountNotificationKind.Message : AccountNotificationKind.Reply);

            string title;
            string body;
            string author;

            if (kind == AccountNotificationKind.Message)
            {
                author = content?.From;
                title = string.IsNullOrWhiteSpace(author) ? "New message" : ("Message from " + author);
                body = !string.IsNullOrWhiteSpace(content?.Subject) ? content.Subject : (content?.Body ?? string.Empty);
            }
            else
            {
                author = content?.Author;
                title = string.IsNullOrWhiteSpace(author) ? "New reply" : (author + " replied");
                body = content?.Comment ?? string.Empty;
            }

            return new AccountNotification
            {
                Id = n.Id,
                Viewed = n.Viewed,
                Kind = kind,
                Title = title,
                Body = body,
                Author = author,
                ImageId = content?.ImageId
            };
        }
    }
}
