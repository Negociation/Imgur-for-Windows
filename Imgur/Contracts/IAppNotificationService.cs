using Imgur.Models;
using System;

namespace Imgur.Contracts
{
    public interface IAppNotificationService
    {

        event EventHandler<Notification> NotificationAdded;
        void AddNotification(Notification notification);
        void RemoveNotification(Notification notification);

    }
}
