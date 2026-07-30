using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Imgur.Background
{
    /// <summary>
    /// Background Task periódica (TimeTrigger) para atualizar Live Tiles.
    /// Entry point: Imgur.Background.LiveTileBackgroundTask
    /// </summary>
    public sealed class LiveTileBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                await LiveTilesService.UpdateLiveTilesTaskAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LiveTileBackgroundTask: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
