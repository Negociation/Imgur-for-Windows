using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Microsoft.Toolkit.Uwp.Notifications;
using Imgur.Contracts;
using Windows.UI.Notifications;

namespace Imgur.Uwp.Services
{
    public class AppLifeCycleService : IAppLifeCycleService
    {
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
            notifyBeforeRestart(reason);
            await CoreApplication.RequestRestartAsync(reason);
        }

        private void notifyBeforeRestart(string reason)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var title = resourceLoader.GetString("NotificationTitleRestartMsg");
            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
            {
                new AdaptiveText()
                {
                    Text = title
                },
                new AdaptiveText()
                {
                    Text = reason
                }
            }
                    }
                }
            };

            // Mostrar a notificação
            ToastNotification toast = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
