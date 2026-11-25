using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Microsoft.Toolkit.Uwp.Notifications;
using Imgur.Contracts;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace Imgur.Uwp.Services
{
    public class AppLifeCycleService : IAppLifeCycleService
    {
        private readonly ISystemInfoProvider _systemInfo;

        public AppLifeCycleService(ISystemInfoProvider systemInfo)
        {
            _systemInfo = systemInfo;
        }

        public async Task RestartAsync(string reason = null)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var genericReason = resourceLoader.GetString("GenericRestartMsg");

            await CoreApplication.RequestRestartAsync(
                reason ?? genericReason
            );
        }

        public async Task RestartToApplyCustomApiSettings()
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();

            var reason = resourceLoader.GetString("AppApiSettingsChangedRestartMsg") ?? "Settings Changed";
            NotifyBeforeRestart(reason);
            await CoreApplication.RequestRestartAsync(reason);
        }

        public async Task RestartToApplyLanguageSettings()
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();

            var body = resourceLoader.GetString("AppLanguageChangedRestartMsg");
            if (string.IsNullOrWhiteSpace(body))
                body = "The app was restarted to apply the new language.";

            bool mobile = _systemInfo != null && _systemInfo.IsMobile();
            if (mobile)
            {
                // Mobile: toast “reabra” + fechar app (override já está salvo).
                var reopenTitle = resourceLoader.GetString("AppLanguageReopenTitle");
                if (string.IsNullOrWhiteSpace(reopenTitle))
                    reopenTitle = "Reopen Imgur?";

                NotifyBeforeRestart(body, reopenTitle);
                try { await Task.Delay(450); } catch { }

                try { Application.Current.Exit(); }
                catch (Exception ex) { Debug.WriteLine("[AppLifeCycle] Mobile Exit: " + ex.Message); }
                return;
            }

            // PC: toast + reinício automático
            var title = resourceLoader.GetString("AppLanguageChangedRestartTitle");
            if (string.IsNullOrWhiteSpace(title))
                title = resourceLoader.GetString("NotificationTitleRestartMsg");

            NotifyBeforeRestart(body, title);
            try { await Task.Delay(200); } catch { }
            await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private static void NotifyBeforeRestart(string reason, string titleOverride = null)
        {
            try
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var title = !string.IsNullOrWhiteSpace(titleOverride)
                    ? titleOverride
                    : resourceLoader.GetString("NotificationTitleRestartMsg");

                if (string.IsNullOrWhiteSpace(title))
                    title = "Restart Required!";
                if (string.IsNullOrWhiteSpace(reason))
                    reason = string.Empty;

                var content = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText() { Text = title },
                                new AdaptiveText() { Text = reason }
                            }
                        }
                    }
                };

                ToastNotificationManager.CreateToastNotifier()
                    .Show(new ToastNotification(content.GetXml()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[AppLifeCycle] toast failed: " + ex.Message);
            }
        }
    }
}
