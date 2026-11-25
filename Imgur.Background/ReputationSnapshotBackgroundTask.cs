using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace Imgur.Background
{
    /// <summary>
    /// TimeTrigger (~60 min) reputation snapshot for lock screen / badge.
    /// Entry point: Imgur.Background.ReputationSnapshotBackgroundTask
    /// </summary>
    public sealed class ReputationSnapshotBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                await ReputationSnapshotService.CaptureTaskAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReputationSnapshotBackgroundTask: " + ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
