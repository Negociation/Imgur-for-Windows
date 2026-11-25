using System;
using System.Collections.Generic;
using Imgur.Contracts;
using Imgur.Helpers;
using Xunit;

namespace Imgur.Tests.Helpers
{
    public class ReputationTrendCalculatorTests
    {
        [Fact]
        public void No_snapshots_means_no_history()
        {
            var result = ReputationTrendCalculator.Calculate(100, "Neutral", null, DateTimeOffset.UtcNow);
            Assert.False(result.HasHistory);
            Assert.Equal(0, result.Delta);
        }

        [Fact]
        public void Uses_oldest_snapshot_within_24h()
        {
            var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
            var snapshots = new List<AccountSnapshotRecord>
            {
                new AccountSnapshotRecord { Reputation = 80, CapturedAt = now.AddHours(-30) },
                new AccountSnapshotRecord { Reputation = 90, CapturedAt = now.AddHours(-20) },
                new AccountSnapshotRecord { Reputation = 95, CapturedAt = now.AddHours(-1) }
            };

            var result = ReputationTrendCalculator.Calculate(100, "Accepted", snapshots, now);
            Assert.True(result.HasHistory);
            Assert.Equal(90, result.BaselineReputation);
            Assert.Equal(10, result.Delta);
        }
    }
}
