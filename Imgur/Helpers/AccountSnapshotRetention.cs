using System;

namespace Imgur.Helpers
{
    /// <summary>
    /// Pure retention policy for account reputation snapshots (48h window).
    /// </summary>
    public static class AccountSnapshotRetention
    {
        public static readonly TimeSpan MaxAge = TimeSpan.FromHours(48);

        public static DateTimeOffset OldestRetainedUtc(DateTimeOffset nowUtc)
        {
            return nowUtc - MaxAge;
        }

        public static bool ShouldPrune(DateTimeOffset capturedAtUtc, DateTimeOffset nowUtc)
        {
            return capturedAtUtc < OldestRetainedUtc(nowUtc);
        }
    }
}
