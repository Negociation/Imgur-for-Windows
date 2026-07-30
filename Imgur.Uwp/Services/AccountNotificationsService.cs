using Imgur.Constants;
using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Adapter DI + registro da Background Task (15 min) de Account Notifications.
    /// </summary>
    public class AccountNotificationsService : IAccountNotificationsService
    {
        public const string TaskName = "ImgurAccountNotification";
        public const string TaskEntryPoint = "Imgur.Background.AccountNotificationBackgroundTask";
        private const uint IntervalMinutes = 15;

        private readonly Imgur.Background.AccountNotificationService _backgroundService =
            new Imgur.Background.AccountNotificationService();

        public async Task OnAccountNotificationsConfigChangedAsync()
        {
            var localSettings = App.Services?.GetService(typeof(ILocalSettings)) as ILocalSettings;

            // Zera o último Id no liga/desliga: ao religar, o toast pode sair de novo.
            localSettings?.Set(LocalSettingsConstants.LastAccountNotificationId, "");

            await SyncBackgroundTaskRegistrationAsync();

            if (localSettings != null && localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications))
            {
                await _backgroundService.TryShowAsync().AsTask();
            }
        }

        public static async Task EnsureBackgroundTaskRegisteredAsync()
        {
            try
            {
                var localSettings = App.Services?.GetService(typeof(ILocalSettings)) as ILocalSettings;
                if (localSettings != null && !localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications))
                {
                    UnregisterBackgroundTask();
                    return;
                }

                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access == BackgroundAccessStatus.DeniedByUser ||
                    access == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    DebugWrite($"Account BT access denied: {access}");
                    return;
                }

                foreach (var entry in BackgroundTaskRegistration.AllTasks)
                {
                    if (entry.Value.Name == TaskName)
                        return;
                }

                var builder = new BackgroundTaskBuilder
                {
                    Name = TaskName,
                    TaskEntryPoint = TaskEntryPoint
                };
                builder.SetTrigger(new TimeTrigger(IntervalMinutes, false));
                builder.Register();
            }
            catch (Exception ex)
            {
                DebugWrite($"Falha ao registrar Account BT: {ex.Message}");
            }
        }

        public static void UnregisterBackgroundTask()
        {
            try
            {
                UnregisterExisting();
            }
            catch (Exception ex)
            {
                DebugWrite($"Falha ao unregister Account BT: {ex.Message}");
            }
        }

        private async Task SyncBackgroundTaskRegistrationAsync()
        {
            var localSettings = App.Services.GetService(typeof(ILocalSettings)) as ILocalSettings;
            bool enabled = localSettings != null && localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications);

            if (enabled)
                await EnsureBackgroundTaskRegisteredAsync();
            else
                UnregisterBackgroundTask();
        }

        private static void UnregisterExisting()
        {
            foreach (var entry in BackgroundTaskRegistration.AllTasks)
            {
                if (entry.Value.Name == TaskName)
                    entry.Value.Unregister(true);
            }
        }

        private static void DebugWrite(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
