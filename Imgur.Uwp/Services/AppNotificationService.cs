using Imgur.Models;
using Imgur.Contracts;
using System;
using System.Collections.Generic;


namespace Imgur.Uwp.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        private List<Notification> notifications = new List<Notification>();

        public event EventHandler<Notification> NotificationAdded;

        public void AddNotification(Notification notification)
        {
            notifications.Add(notification);
            NotificationAdded?.Invoke(this, notification);
        }

        public void RemoveNotification(Notification notification)
        {
            notifications.Remove(notification);
        }
    }
}
