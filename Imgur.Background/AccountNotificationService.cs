using Imgur.Api.Services.Models.Response.Notification;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.UI.Notifications;

namespace Imgur.Background
{
    /// <summary>
    /// Toast das notificações da conta Imgur (replies / messages), a cada ~15 min.
    /// Requer Bearer (PasswordVault) + setting AccountNotifications.
    /// </summary>
    public sealed class AccountNotificationService
    {
        private const string ToastTag = "imgur_account_notif";
        private const string ToastGroup = "imgur";
        private const string VaultResource = "ImgurOAuth";

        public IAsyncAction TryShowAsync()
        {
            return TryShowCoreAsync().AsAsyncAction();
        }

        internal static Task TryShowTaskAsync()
        {
            return TryShowCoreAsync();
        }

        private static async Task TryShowCoreAsync()
        {
            ILocalSettings localSettings = new BackgroundLocalSettings();

            if (!localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications))
            {
                Debug.WriteLine("Account notifications desativadas");
                return;
            }

            var accessToken = TryGetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.WriteLine("Account notifications: sem token (não logado)");
                return;
            }

            try
            {
                var credentials = new ImgurApiCredentialsProvider(localSettings);
                var api = new Imgur.Api.Services.Actions.AccountService(credentials.ClientId);
                api.SetAccessTokenProvider(() => accessToken);

                var response = await api.GetNotificationsAsync(onlyNew: true);
                if (response == null || !response.Success || response.Data == null)
                {
                    Debug.WriteLine("Account notifications: falha ao buscar");
                    return;
                }

                var newest = PickNewest(response.Data);
                if (newest == null)
                {
                    Debug.WriteLine("Account notifications: nenhuma nova");
                    return;
                }

                var lastId = localSettings.Get<string>(LocalSettingsConstants.LastAccountNotificationId);
                var newestId = newest.Id.ToString();
                if (string.Equals(lastId, newestId, StringComparison.Ordinal))
                {
                    Debug.WriteLine($"Account notifications: skip, mesmo Id ({newestId})");
                    return;
                }

                ShowToast(newest);
                localSettings.Set(LocalSettingsConstants.LastAccountNotificationId, newestId);
                Debug.WriteLine($"Account notifications: toast para {newestId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Account notifications: {ex.Message}");
            }
        }

        private static NotificationResponse PickNewest(AccountNotificationsData data)
        {
            var all = new List<NotificationResponse>();
            if (data.Replies != null)
                all.AddRange(data.Replies);
            if (data.Messages != null)
                all.AddRange(data.Messages);

            return all.OrderByDescending(n => n.Id).FirstOrDefault();
        }

        private static void ShowToast(NotificationResponse notification)
        {
            var content = notification.Content;
            bool isMessage = content != null &&
                (!string.IsNullOrEmpty(content.From) || !string.IsNullOrEmpty(content.Subject) || !string.IsNullOrEmpty(content.Body))
                && string.IsNullOrEmpty(content.Comment);

            string title;
            string body;

            if (isMessage)
            {
                title = string.IsNullOrWhiteSpace(content.From)
                    ? "New message on Imgur"
                    : $"Message from {content.From}";
                body = !string.IsNullOrWhiteSpace(content.Subject)
                    ? content.Subject
                    : (content.Body ?? "");
            }
            else
            {
                var author = content?.Author;
                title = string.IsNullOrWhiteSpace(author)
                    ? "New reply on Imgur"
                    : $"{author} replied";
                body = content?.Comment ?? "";
            }

            if (body.Length > 120)
                body = body.Substring(0, 117) + "...";

            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintStyle = AdaptiveTextStyle.Base
                            },
                            new AdaptiveText()
                            {
                                Text = body,
                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                            }
                        }
                    }
                }
            };

            var toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = ToastTag,
                Group = ToastGroup
            };

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private static string TryGetAccessToken()
        {
            try
            {
                var vault = new PasswordVault();
                var cred = vault.FindAllByResource(VaultResource)[0];
                cred.RetrievePassword();
                var parts = cred.Password.Split('|');
                return parts.Length > 0 ? parts[0] : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
