using Imgur.Models;
using Imgur.Contracts;
using System;
using System.Collections.Generic;
using Imgur.ViewModels.Media;
using System.Diagnostics;
using Imgur.Helpers;
using Windows.ApplicationModel.Resources;
using Imgur.Enums;
using Imgur.ViewModels.Account;
using Imgur.ViewModels.Tags;

namespace Imgur.Uwp.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        private List<NotificationViewModel> notifications = new List<NotificationViewModel>();

        public event EventHandler<NotificationViewModel> NotificationAdded;

        public void AddMediaClipboardNotification(MediaViewModel media, ImgurUrlType imgurUrlType = ImgurUrlType.Image)
        {

            var resourceLoader = ResourceLoader.GetForCurrentView();


            var notificationVm = new NotificationViewModel();
            notificationVm.Title = notificationVm.Title = resourceLoader.GetString($"notification_open_{imgurUrlType.ToString().ToLowerInvariant()}");
            notificationVm.Message =
                string.IsNullOrWhiteSpace(media.CurrentMedia.Title)
                    ? media.CurrentMedia.Link
                    : media.CurrentMedia.Title;

            notificationVm.Extra =
                string.IsNullOrWhiteSpace(media.CurrentMedia.AccountId)
                    ? resourceLoader.GetString("HiddenAuthor")
                    : media.CurrentMedia.AccountId;
            notificationVm.ImageUrl = media.CurrentMedia.CoverPlaceholder;
            notificationVm.ActionCommand = new RelayCommand(() =>
            {
                media.NavigateMediaCommand.Execute(null);
            });


            notifications.Add(notificationVm);
            NotificationAdded?.Invoke(this, notificationVm);
        }


        public void AddNotification(NotificationViewModel notification)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            notification.Title = notification.Title ?? resourceLoader.GetString("notification_generic");
            notification.Message = resourceLoader.GetString(notification.Message);

            notifications.Add(notification);
            NotificationAdded?.Invoke(this, notification);
        }

        public void AddApiWarningNotification(string status)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();

            var notificationVm = new NotificationViewModel();
            notificationVm.Title = resourceLoader.GetString("notification_api_warning");
            notificationVm.Message = resourceLoader.GetString("notification_api_warning_content");
            notificationVm.Extra = status;
            notifications.Add(notificationVm);
            NotificationAdded?.Invoke(this, notificationVm);
        }

        public void RemoveNotification(NotificationViewModel notification)
        {
            notifications.Remove(notification);
        }

        public void AddTagClipboardNotification(TagViewModel tag)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();

            var notificationVm = new NotificationViewModel();
            notificationVm.Title = notificationVm.Title = resourceLoader.GetString("notification_open_tag");
            notificationVm.Message = tag.CurrentTag.Title;
                

            notificationVm.Extra = tag.CurrentTag.Description;
            notificationVm.ImageUrl = tag.CurrentTag.BackgroundTile;

            notificationVm.ActionCommand = new RelayCommand(() =>
            {
                tag.NavigateTagCommand.Execute(null);
            });

            notifications.Add(notificationVm);
            NotificationAdded?.Invoke(this, notificationVm);
        }

        public void AddUserClipboardNotification(AccountViewModel account)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var notificationVm = new NotificationViewModel();
            notificationVm.Title = notificationVm.Title = resourceLoader.GetString("notification_open_account");
            notificationVm.Message =  account.UserAccountInfo.Username;
            notificationVm.Extra = account.UserAccountInfo.ReputationName;
            notificationVm.ImageUrl = account.UserAccountInfo.Avatar;
            notificationVm.IsImageAvatarInfo = true;

            notificationVm.ActionCommand = new RelayCommand(() =>
            {
                account.NavigateAccountViewCommand.Execute(null);
            });

            notifications.Add(notificationVm);
            NotificationAdded?.Invoke(this, notificationVm);
        }
    }
}
