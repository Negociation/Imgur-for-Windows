using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public sealed class AccountSnapshotRecord
    {
        public string Username { get; set; }
        public int Reputation { get; set; }
        public string ReputationName { get; set; }
        public DateTimeOffset CapturedAt { get; set; }
    }

    public sealed class NotificationSeenRecord
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public DateTimeOffset SeenAt { get; set; }
    }

    public sealed class UploadHistoryRecord
    {
        public string Link { get; set; }
        public string DeleteHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public interface IImgurLocalStore
    {
        Task InitializeAsync();

        Task AddAccountSnapshotAsync(AccountSnapshotRecord snapshot);
        Task<IReadOnlyList<AccountSnapshotRecord>> GetAccountSnapshotsAsync(string username, DateTimeOffset sinceUtc);
        Task PruneAccountSnapshotsOlderThanAsync(TimeSpan maxAge);

        Task<bool> TryMarkNotificationSeenAsync(NotificationSeenRecord seen);
        Task<bool> HasSeenNotificationAsync(string id);

        Task AddUploadHistoryAsync(UploadHistoryRecord upload);
        Task<IReadOnlyList<UploadHistoryRecord>> GetUploadHistoryAsync(int limit);
    }
}
