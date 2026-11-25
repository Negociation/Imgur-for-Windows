using Imgur.Api.Services.Contracts;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Foreground 15‑min poll + delivery: InApp when visible, toast when background.
    /// Background task checks <see cref="LocalSettingsConstants.AppIsForeground"/> to skip toast.
    /// </summary>
    public class NotificationRouter : INotificationRouter
    {
        public const string ToastTag = "imgur_account_notif";
        public const string ToastGroup = "imgur";
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

        private readonly IAccountService _accountApi;
        private readonly IAccountInboxService _inbox;
        private readonly IAppNotificationService _inApp;
        private readonly ILocalSettings _localSettings;
        private readonly IUserContext _userContext;
        private readonly IDispatcher _dispatcher;

        private DispatcherTimer _timer;
        private string _lastDeliveredId = string.Empty;
        private bool _isForeground = true;

        public NotificationRouter(
            IAccountService accountApi,
            IAccountInboxService inbox,
            IAppNotificationService inApp,
            ILocalSettings localSettings,
            IUserContext userContext,
            IDispatcher dispatcher)
        {
            _accountApi = accountApi;
            _inbox = inbox;
            _inApp = inApp;
            _localSettings = localSettings;
            _userContext = userContext;
            _dispatcher = dispatcher;
        }

        public bool IsForeground => _isForeground;

        public void SetForeground(bool isForeground)
        {
            var wasForeground = _isForeground;
            _isForeground = isForeground;
            _localSettings.Set(LocalSettingsConstants.AppIsForeground, isForeground);

            // Só na transição: OnLeavingBackground e OnResuming chamam SetForeground(true) em sequência.
            // A tarefa em segundo plano pode ter marcado notificações como novas enquanto o app estava suspenso.
            if (ForegroundTransition.IsReturningToForeground(wasForeground, isForeground))
                _ = _inbox.RefreshUnreadAsync();
        }

        public async Task StartAsync()
        {
            SetForeground(true);
            _lastDeliveredId = _localSettings.Get<string>(LocalSettingsConstants.LastAccountNotificationId) ?? string.Empty;

            await _inbox.RefreshUnreadAsync();
            await PollOnceAsync(deliverNew: true);

            if (_timer == null)
            {
                _timer = new DispatcherTimer { Interval = PollInterval };
                _timer.Tick += async (s, e) => await PollOnceAsync(deliverNew: true);
            }

            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            SetForeground(false);
        }

        public void Deliver(AccountNotification notification)
        {
            if (notification == null)
                return;

            var id = notification.Id.ToString();
            if (string.Equals(_lastDeliveredId, id, StringComparison.Ordinal))
                return;

            _lastDeliveredId = id;
            _localSettings.Set(LocalSettingsConstants.LastAccountNotificationId, id);

            if (_isForeground)
            {
                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    _inApp.AddPlainTextNotification(
                        notification.Title,
                        notification.Body,
                        notification.AvatarUrl);
                });
                _ = _inbox.RefreshUnreadAsync();
            }
            else
            {
                ShowToast(notification);
            }
        }

        private async Task PollOnceAsync(bool deliverNew)
        {
            if (_userContext == null || !_userContext.IsAuthenticated)
                return;

            if (!_localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications))
                return;

            try
            {
                var response = await _accountApi.GetNotificationsAsync(onlyNew: true);
                if (response == null || !response.Success || response.Data == null)
                    return;

                var flat = AccountNotificationMapper.Flatten(response.Data);
                await _inbox.RefreshUnreadAsync();

                if (!deliverNew || flat.Count == 0)
                    return;

                var newest = flat.First();
                if (string.Equals(_lastDeliveredId, newest.Id.ToString(), StringComparison.Ordinal))
                    return;

                Deliver(newest);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NotificationRouter] Poll: {ex.Message}");
            }
        }

        public static void ShowToast(AccountNotification notification)
        {
            if (notification == null)
                return;

            var body = notification.Body ?? string.Empty;
            if (body.Length > 120)
                body = body.Substring(0, 117) + "...";

            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = notification.Title ?? "Imgur",
                                HintStyle = AdaptiveTextStyle.Base
                            },
                            new AdaptiveText
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
    }
}
