using Imgur.Models;
using Imgur.Contracts;
using System;
using System.Collections.Generic;
using Imgur.ViewModels.Media;
using System.Diagnostics;
using Imgur.Helpers;
using Windows.ApplicationModel.Resources;

namespace Imgur.Uwp.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        private List<NotificationViewModel> notifications = new List<NotificationViewModel>();

        public event EventHandler<NotificationViewModel> NotificationAdded;

        public void AddMediaClipboardNotification(MediaViewModel media)
        {

            var resourceLoader = ResourceLoader.GetForCurrentView();


            var notificationVm = new NotificationViewModel();
            notificationVm.Title = resourceLoader.GetString("notification_open_gallery");
            notificationVm.Message = media.CurrentMedia.Title;
            notificationVm.Extra = media.CurrentMedia.AccountId;
            notificationVm.ImageUrl = media.CurrentMedia.CoverPlaceholder;
            notificationVm.ActionCommand = new RelayCommand(() =>
            {
                media.NavigateMediaCommand.Execute(null);
            });
            this.AddNotification(notificationVm);
        }


        public void AddNotification(NotificationViewModel notification)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            notification.Title = notification.Title ?? resourceLoader.GetString("notification_generic");
            notification.Message = resourceLoader.GetString(notification.Message);

            notifications.Add(notification);
            NotificationAdded?.Invoke(this, notification);
        }

        public void RemoveNotification(NotificationViewModel notification)
        {
            notifications.Remove(notification);
        }
    }
}
