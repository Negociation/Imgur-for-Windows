using System;

namespace Imgur.Helpers
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right
    }

    public static class SwipeGesture
    {
        public const double DistanceThreshold = 80;
        public const double FlickDistanceThreshold = 30;

        // ManipulationVelocities do UWP são em DIPs por milissegundo.
        public const double FlickVelocityThreshold = 0.8;

        public const double EdgeWidth = 20;

        public static SwipeDirection Classify(double translationX, double velocityX)
        {
            var distance = Math.Abs(translationX);
            var isLongDrag = distance >= DistanceThreshold;
            var isFlick = distance >= FlickDistanceThreshold && Math.Abs(velocityX) >= FlickVelocityThreshold;

            if (!isLongDrag && !isFlick)
                return SwipeDirection.None;

            return translationX < 0 ? SwipeDirection.Left : SwipeDirection.Right;
        }

        /// <summary>
        /// Swipe para a esquerda avança, para a direita volta; circular como o Pivot.
        /// </summary>
        public static int CycleIndex(int currentIndex, int count, SwipeDirection direction)
        {
            if (count <= 0 || direction == SwipeDirection.None)
                return currentIndex;

            var step = direction == SwipeDirection.Left ? 1 : -1;
            return ((currentIndex + step) % count + count) % count;
        }

        public static bool IsEdgeSwipeToOpen(double startX, double translationX, double velocityX)
        {
            return startX <= EdgeWidth && Classify(translationX, velocityX) == SwipeDirection.Right;
        }
    }
}
