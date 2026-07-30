using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Imgur.Background
{
    /// <summary>
    /// Background Task (~15 min) para notificações da conta Imgur.
    /// Entry point: Imgur.Background.AccountNotificationBackgroundTask
    /// </summary>
    public sealed class AccountNotificationBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                await AccountNotificationService.TryShowTaskAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AccountNotificationBackgroundTask: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
