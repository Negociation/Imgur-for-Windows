using Imgur.Helpers;
using Imgur.Contracts;
using Xunit;

namespace Imgur.Tests.ViewModels
{
    public class StartupLaunchPresentationTests
    {
        [Theory]
        [InlineData(StartupLaunchState.Enabled, true)]
        [InlineData(StartupLaunchState.EnabledByPolicy, true)]
        [InlineData(StartupLaunchState.Disabled, false)]
        [InlineData(StartupLaunchState.DisabledByUser, false)]
        [InlineData(StartupLaunchState.NotSupported, false)]
        public void IsOn_maps_expected_states(StartupLaunchState state, bool expected)
        {
            Assert.Equal(expected, StartupLaunchPresentation.IsOn(state));
        }

        [Theory]
        [InlineData(StartupLaunchState.Enabled, true)]
        [InlineData(StartupLaunchState.Disabled, true)]
        [InlineData(StartupLaunchState.EnabledByPolicy, true)]
        [InlineData(StartupLaunchState.DisabledByUser, false)]
        [InlineData(StartupLaunchState.DisabledByPolicy, false)]
        [InlineData(StartupLaunchState.NotSupported, false)]
        public void CanToggle_maps_expected_states(StartupLaunchState state, bool expected)
        {
            Assert.Equal(expected, StartupLaunchPresentation.CanToggle(state));
        }
    }
}
