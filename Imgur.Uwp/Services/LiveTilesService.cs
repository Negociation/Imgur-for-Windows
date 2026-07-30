using Imgur.Constants;
using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Adapter DI: ILiveTilesService → Imgur.Background.LiveTilesService
    /// + registro/unregister da Background Task (30 min).
    /// </summary>
    public class LiveTilesService : ILiveTilesService
    {
        public const string TaskName = "ImgurLiveTileUpdate";
        public const string TaskEntryPoint = "Imgur.Background.LiveTileBackgroundTask";
        private const uint IntervalMinutes = 30;

        private readonly Imgur.Background.LiveTilesService _backgroundService = new Imgur.Background.LiveTilesService();

        public Task UpdateLiveTilesAsync()
        {
            return _backgroundService.UpdateLiveTilesAsync().AsTask();
        }

        public async Task OnLiveTilesConfigChangedAsync()
        {
            await UpdateLiveTilesAsync();
            await SyncBackgroundTaskRegistrationAsync();
        }

        public static async Task EnsureBackgroundTaskRegisteredAsync()
        {
            try
            {
                var localSettings = App.Services?.GetService(typeof(ILocalSettings)) as ILocalSettings;
                if (localSettings != null && !localSettings.Get<bool>(LocalSettingsConstants.LiveTiles))
                {
                    UnregisterBackgroundTask();
                    return;
                }

                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access == BackgroundAccessStatus.DeniedByUser ||
                    access == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    DebugWrite($"Live Tile BT access denied: {access}");
                    return;
                }

                // Já registrada
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
                DebugWrite($"Falha ao registrar Live Tile BT: {ex.Message}");
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
                DebugWrite($"Falha ao unregister Live Tile BT: {ex.Message}");
            }
        }

        private async Task SyncBackgroundTaskRegistrationAsync()
        {
            var localSettings = App.Services.GetService(typeof(ILocalSettings)) as ILocalSettings;
            bool enabled = localSettings != null && localSettings.Get<bool>(LocalSettingsConstants.LiveTiles);

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
                {
                    entry.Value.Unregister(true);
                }
            }
        }

        private static void DebugWrite(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
