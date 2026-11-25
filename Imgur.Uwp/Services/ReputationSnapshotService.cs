using Imgur.Contracts;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Imgur.Uwp.Services
{
    public interface IReputationSnapshotService
    {
        Task CaptureNowAsync();
        Task OnConfigChangedAsync();
    }

    public sealed class ReputationSnapshotService : IReputationSnapshotService
    {
        public const string TaskName = "ImgurReputationSnapshot";
        public const string TaskEntryPoint = "Imgur.Background.ReputationSnapshotBackgroundTask";
        private const uint IntervalMinutes = 60;

        private readonly Imgur.Background.ReputationSnapshotService _background =
            new Imgur.Background.ReputationSnapshotService();

        public Task CaptureNowAsync()
        {
            return _background.CaptureAsync().AsTask();
        }

        public async Task OnConfigChangedAsync()
        {
            await CaptureNowAsync();
            await EnsureBackgroundTaskRegisteredAsync();
        }

        public static async Task EnsureBackgroundTaskRegisteredAsync()
        {
            try
            {
                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access == BackgroundAccessStatus.DeniedByUser ||
                    access == BackgroundAccessStatus.DeniedBySystemPolicy)
                {
                    Debug.WriteLine("Reputation BT access denied: " + access);
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
                Debug.WriteLine("Reputation BT register failed: " + ex.Message);
            }
        }
    }
}
