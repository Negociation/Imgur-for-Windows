using Imgur.Constants;
using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Adapter DI + registro da Background Task (6h) de Remind Notifications.
    /// </summary>
    public class RemindNotificationsService : IRemindNotificationsService
    {
        public const string TaskName = "ImgurRemindNotification";
        public const string TaskEntryPoint = "Imgur.Background.RemindNotificationBackgroundTask";
        private const uint IntervalMinutes = 360; // 6 horas

        private readonly Imgur.Background.RemindNotificationService _backgroundService =
            new Imgur.Background.RemindNotificationService();

        public async Task OnRemindNotificationsConfigChangedAsync()
        {
            var localSettings = App.Services?.GetService(typeof(ILocalSettings)) as ILocalSettings;

            // Zera o último Id no liga/desliga: ao religar, o toast pode sair de novo
            // (prova de vida em Release sem precisar limpar settings manualmente).
            localSettings?.Set(LocalSettingsConstants.LastRemindNotificationMediaId, "");

            await SyncBackgroundTaskRegistrationAsync();

            if (localSettings != null && localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications))
            {
                await _backgroundService.TryShowAsync().AsTask();
            }
        }

        public static async Task EnsureBackgroundTaskRegisteredAsync()
        {
            try
            {
                var localSettings = App.Services?.GetService(typeof(ILocalSettings)) as ILocalSettings;
                if (localSettings != null && !localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications))
                {
                    UnregisterBackgroundTask();
                    return;
                }

                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access == BackgroundAccessStatus.DeniedByUser ||
                    access == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    DebugWrite($"Remind BT access denied: {access}");
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
                DebugWrite($"Falha ao registrar Remind BT: {ex.Message}");
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
                DebugWrite($"Falha ao unregister Remind BT: {ex.Message}");
            }
        }

        private async Task SyncBackgroundTaskRegistrationAsync()
        {
            var localSettings = App.Services.GetService(typeof(ILocalSettings)) as ILocalSettings;
            bool enabled = localSettings != null && localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications);

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
