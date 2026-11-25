using Imgur.Contracts;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.System.Profile;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Start with Windows via package startup task (uap5:StartupTask, TaskId <see cref="TaskId"/>).
    /// Desktop 1709+ only; Mobile / older builds report <see cref="StartupLaunchState.NotSupported"/>.
    /// </summary>
    public sealed class StartupLaunchService : IStartupLaunchService
    {
        public const string TaskId = "ImgurStartup";

        private const string DesktopFamily = "Windows.Desktop";
        private readonly Lazy<bool> _isSupported;

        public StartupLaunchService()
        {
            _isSupported = new Lazy<bool>(DetectSupport);
        }

        public bool IsSupported => _isSupported.Value;

        public async Task<StartupLaunchState> GetStateAsync()
        {
            StartupTask task = await TryGetTaskAsync();
            return task == null ? StartupLaunchState.NotSupported : Map(task.State);
        }

        public async Task<StartupLaunchState> SetEnabledAsync(bool enabled)
        {
            StartupTask task = await TryGetTaskAsync();
            if (task == null)
                return StartupLaunchState.NotSupported;

            StartupLaunchState before = Map(task.State);
            if (enabled && (before == StartupLaunchState.Enabled || before == StartupLaunchState.EnabledByPolicy))
                return before;

            if (!enabled && before == StartupLaunchState.Disabled)
                return before;

            if (enabled &&
                (before == StartupLaunchState.DisabledByUser || before == StartupLaunchState.DisabledByPolicy))
            {
                return before;
            }

            try
            {
                if (enabled)
                    return Map(await task.RequestEnableAsync());

                task.Disable();
                return Map(task.State);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupLaunchService] set-failed: {ex.Message}");
                return Map(task.State);
            }
        }

        private async Task<StartupTask> TryGetTaskAsync()
        {
            if (!IsSupported)
                return null;

            try
            {
                return await StartupTask.GetAsync(TaskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupLaunchService] get-failed: {ex.Message}");
                return null;
            }
        }

        private static bool DetectSupport()
        {
            try
            {
                return string.Equals(
                           AnalyticsInfo.VersionInfo.DeviceFamily,
                           DesktopFamily,
                           StringComparison.OrdinalIgnoreCase) &&
                       ApiInformation.IsTypePresent("Windows.ApplicationModel.StartupTask");
            }
            catch
            {
                return false;
            }
        }

        private static StartupLaunchState Map(StartupTaskState state)
        {
            switch (state)
            {
                case StartupTaskState.Enabled:
                    return StartupLaunchState.Enabled;
                case StartupTaskState.DisabledByUser:
                    return StartupLaunchState.DisabledByUser;
                case StartupTaskState.DisabledByPolicy:
                    return StartupLaunchState.DisabledByPolicy;
                case StartupTaskState.EnabledByPolicy:
                    return StartupLaunchState.EnabledByPolicy;
                default:
                    return StartupLaunchState.Disabled;
            }
        }
    }
}
