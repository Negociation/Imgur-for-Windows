using System;
using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class AccountSnapshotRetentionTests
    {
        [Fact]
        public void OldestRetainedUtc_is_48_hours_before_now()
        {
            var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
            Assert.Equal(now.AddHours(-48), AccountSnapshotRetention.OldestRetainedUtc(now));
        }

        [Fact]
        public void ShouldPrune_true_when_older_than_48h()
        {
            var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
            var old = now.AddHours(-49);
            Assert.True(AccountSnapshotRetention.ShouldPrune(old, now));
        }

        [Fact]
        public void ShouldPrune_false_when_within_48h()
        {
            var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
            var recent = now.AddHours(-47);
            Assert.False(AccountSnapshotRetention.ShouldPrune(recent, now));
        }
    }
}
