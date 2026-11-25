using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class SwipeGestureTests
    {
        [Fact]
        public void Short_slow_drag_is_not_a_swipe()
        {
            Assert.Equal(SwipeDirection.None, SwipeGesture.Classify(translationX: 40, velocityX: 0.1));
        }

        [Fact]
        public void Long_drag_to_the_left_is_a_left_swipe()
        {
            Assert.Equal(SwipeDirection.Left, SwipeGesture.Classify(translationX: -120, velocityX: 0.1));
        }

        [Fact]
        public void Long_drag_to_the_right_is_a_right_swipe()
        {
            Assert.Equal(SwipeDirection.Right, SwipeGesture.Classify(translationX: 120, velocityX: 0.1));
        }

        [Fact]
        public void Quick_short_flick_counts_as_swipe()
        {
            Assert.Equal(SwipeDirection.Left, SwipeGesture.Classify(translationX: -40, velocityX: -1.5));
        }

        [Fact]
        public void Tiny_flick_below_minimum_distance_is_ignored()
        {
            Assert.Equal(SwipeDirection.None, SwipeGesture.Classify(translationX: -10, velocityX: -3));
        }

        [Fact]
        public void Left_swipe_moves_to_next_section()
        {
            Assert.Equal(1, SwipeGesture.CycleIndex(currentIndex: 0, count: 3, direction: SwipeDirection.Left));
        }

        [Fact]
        public void Left_swipe_on_last_section_wraps_to_first_like_pivot()
        {
            Assert.Equal(0, SwipeGesture.CycleIndex(currentIndex: 2, count: 3, direction: SwipeDirection.Left));
        }

        [Fact]
        public void Right_swipe_on_first_section_wraps_to_last_like_pivot()
        {
            Assert.Equal(2, SwipeGesture.CycleIndex(currentIndex: 0, count: 3, direction: SwipeDirection.Right));
        }

        [Fact]
        public void No_direction_or_empty_sections_keeps_index()
        {
            Assert.Equal(1, SwipeGesture.CycleIndex(currentIndex: 1, count: 3, direction: SwipeDirection.None));
            Assert.Equal(0, SwipeGesture.CycleIndex(currentIndex: 0, count: 0, direction: SwipeDirection.Left));
        }

        [Fact]
        public void Right_swipe_starting_at_left_edge_opens_pane()
        {
            Assert.True(SwipeGesture.IsEdgeSwipeToOpen(startX: 8, translationX: 120, velocityX: 0.2));
        }

        [Fact]
        public void Right_swipe_starting_away_from_edge_does_not_open_pane()
        {
            Assert.False(SwipeGesture.IsEdgeSwipeToOpen(startX: 150, translationX: 120, velocityX: 0.2));
        }

        [Fact]
        public void Left_swipe_from_edge_does_not_open_pane()
        {
            Assert.False(SwipeGesture.IsEdgeSwipeToOpen(startX: 8, translationX: -120, velocityX: -0.2));
        }
    }
}
