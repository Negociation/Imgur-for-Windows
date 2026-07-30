using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Imgur.Background
{
    /// <summary>
    /// Background Task (~6h) para toast do Top do dia.
    /// Entry point: Imgur.Background.RemindNotificationBackgroundTask
    /// </summary>
    public sealed class RemindNotificationBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                await RemindNotificationService.TryShowTaskAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RemindNotificationBackgroundTask: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
