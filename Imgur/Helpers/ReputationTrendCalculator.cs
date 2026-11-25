using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgur.Helpers
{
    public sealed class ReputationTrendResult
    {
        public bool HasHistory { get; set; }
        public int CurrentReputation { get; set; }
        public int BaselineReputation { get; set; }
        public int Delta { get; set; }
        public string ReputationName { get; set; }
    }

    /// <summary>
    /// Pure calculator: delta = current − oldest snapshot within the last 24h.
    /// </summary>
    public static class ReputationTrendCalculator
    {
        public static readonly TimeSpan Window = TimeSpan.FromHours(24);

        public static ReputationTrendResult Calculate(
            int currentReputation,
            string reputationName,
            IReadOnlyList<AccountSnapshotRecord> snapshots,
            DateTimeOffset nowUtc)
        {
            var result = new ReputationTrendResult
            {
                CurrentReputation = currentReputation,
                ReputationName = reputationName ?? string.Empty,
                BaselineReputation = currentReputation,
                Delta = 0,
                HasHistory = false
            };

            if (snapshots == null || snapshots.Count == 0)
                return result;

            DateTimeOffset oldestAllowed = nowUtc - Window;
            AccountSnapshotRecord baseline = snapshots
                .Where(s => s != null && s.CapturedAt >= oldestAllowed)
                .OrderBy(s => s.CapturedAt)
                .FirstOrDefault();

            if (baseline == null)
                return result;

            result.HasHistory = true;
            result.BaselineReputation = baseline.Reputation;
            result.Delta = currentReputation - baseline.Reputation;
            return result;
        }
    }
}
