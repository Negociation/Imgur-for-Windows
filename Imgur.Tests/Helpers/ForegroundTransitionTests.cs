using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class ForegroundTransitionTests
    {
        [Fact]
        public void Returning_from_background_is_a_transition()
        {
            Assert.True(ForegroundTransition.IsReturningToForeground(wasForeground: false, isForeground: true));
        }

        [Fact]
        public void Repeated_foreground_signal_is_not_a_transition()
        {
            // OnLeavingBackground e OnResuming chamam SetForeground(true) em sequência.
            Assert.False(ForegroundTransition.IsReturningToForeground(wasForeground: true, isForeground: true));
        }

        [Fact]
        public void Going_to_background_is_not_a_transition_to_foreground()
        {
            Assert.False(ForegroundTransition.IsReturningToForeground(wasForeground: true, isForeground: false));
        }

        [Fact]
        public void Staying_in_background_is_not_a_transition()
        {
            Assert.False(ForegroundTransition.IsReturningToForeground(wasForeground: false, isForeground: false));
        }
    }
}
