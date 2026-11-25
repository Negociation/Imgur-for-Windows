namespace Imgur.Helpers
{
    public static class ForegroundTransition
    {
        public static bool IsReturningToForeground(bool wasForeground, bool isForeground)
        {
            return isForeground && !wasForeground;
        }
    }
}
