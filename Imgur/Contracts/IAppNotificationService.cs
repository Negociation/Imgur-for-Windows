using Imgur.Enums;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Media;
using Imgur.ViewModels.Tags;
using System;

namespace Imgur.Contracts
{
    public interface IAppNotificationService
    {

        event EventHandler<NotificationViewModel> NotificationAdded;

        void AddNotification(NotificationViewModel notification);
        void RemoveNotification(NotificationViewModel notification);

        void AddMediaClipboardNotification(MediaViewModel media, ImgurUrlType imgurUrlType = ImgurUrlType.Image);

        void AddTagClipboardNotification(TagViewModel tag);

        void AddUserClipboardNotification(AccountViewModel tag);

        void AddApiWarningNotification(string status);
    }
}
