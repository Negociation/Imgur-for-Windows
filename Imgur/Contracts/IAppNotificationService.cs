using Imgur.Models;
using Imgur.ViewModels.Media;
using System;

namespace Imgur.Contracts
{
    public interface IAppNotificationService
    {

        event EventHandler<NotificationViewModel> NotificationAdded;

        void AddNotification(NotificationViewModel notification);
        void RemoveNotification(NotificationViewModel notification);

        void AddMediaClipboardNotification(MediaViewModel media);
    }
}
