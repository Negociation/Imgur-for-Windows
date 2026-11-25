using Imgur.Contracts;

namespace Imgur.Helpers
{
    public static class StartupLaunchPresentation
    {
        public static bool IsOn(StartupLaunchState state)
        {
            return state == StartupLaunchState.Enabled || state == StartupLaunchState.EnabledByPolicy;
        }

        public static bool CanToggle(StartupLaunchState state)
        {
            return state == StartupLaunchState.Enabled ||
                   state == StartupLaunchState.Disabled ||
                   state == StartupLaunchState.EnabledByPolicy;
        }
    }
}
