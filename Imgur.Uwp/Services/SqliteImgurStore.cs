using Imgur.Contracts;
using Imgur.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Imgur.Uwp.Services
{
    public sealed class SqliteImgurStore : IImgurLocalStore
    {
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private SQLiteAsyncConnection _db;
        private bool _initialized;

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_initialized)
                    return;

                SQLitePCL.Batteries_V2.Init();

                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "imgur.db");
                _db = new SQLiteAsyncConnection(path);
                await _db.ExecuteAsync("PRAGMA journal_mode=WAL;").ConfigureAwait(false);
                await _db.CreateTableAsync<AccountSnapshotEntity>().ConfigureAwait(false);
                await _db.CreateTableAsync<NotificationSeenEntity>().ConfigureAwait(false);
                await _db.CreateTableAsync<UploadHistoryEntity>().ConfigureAwait(false);
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImgurLocalStore] Initialize failed: {ex.Message}");
                throw;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task AddAccountSnapshotAsync(AccountSnapshotRecord snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Username))
                return;

            await EnsureInitializedAsync().ConfigureAwait(false);

            var entity = new AccountSnapshotEntity
            {
                Username = snapshot.Username.Trim(),
                Reputation = snapshot.Reputation,
                ReputationName = snapshot.ReputationName ?? string.Empty,
                CapturedAt = snapshot.CapturedAt.ToUniversalTime().ToUnixTimeSeconds()
            };

            await _db.InsertAsync(entity).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<AccountSnapshotRecord>> GetAccountSnapshotsAsync(string username, DateTimeOffset sinceUtc)
        {
            if (string.IsNullOrWhiteSpace(username))
                return new List<AccountSnapshotRecord>();

            await EnsureInitializedAsync().ConfigureAwait(false);

            long since = sinceUtc.ToUniversalTime().ToUnixTimeSeconds();
            string user = username.Trim();
            var rows = await _db.Table<AccountSnapshotEntity>()
                .Where(r => r.Username == user && r.CapturedAt >= since)
                .OrderBy(r => r.CapturedAt)
                .ToListAsync()
                .ConfigureAwait(false);

            return rows.Select(ToSnapshotRecord).ToList();
        }

        public async Task PruneAccountSnapshotsOlderThanAsync(TimeSpan maxAge)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);

            var cutoff = DateTimeOffset.UtcNow - maxAge;
            long cutoffUnix = cutoff.ToUnixTimeSeconds();
            await _db.ExecuteAsync(
                "DELETE FROM AccountSnapshot WHERE CapturedAt < ?",
                cutoffUnix).ConfigureAwait(false);
        }

        public async Task<bool> TryMarkNotificationSeenAsync(NotificationSeenRecord seen)
        {
            if (seen == null || string.IsNullOrWhiteSpace(seen.Id))
                return false;

            await EnsureInitializedAsync().ConfigureAwait(false);

            try
            {
                var changes = await _db.ExecuteAsync(
                    "INSERT OR IGNORE INTO NotificationSeen (Id, Kind, SeenAt) VALUES (?, ?, ?)",
                    seen.Id,
                    seen.Kind ?? string.Empty,
                    seen.SeenAt.ToUniversalTime().ToUnixTimeSeconds()).ConfigureAwait(false);
                return changes > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImgurLocalStore] TryMarkNotificationSeen failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasSeenNotificationAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            await EnsureInitializedAsync().ConfigureAwait(false);
            var row = await _db.FindAsync<NotificationSeenEntity>(id).ConfigureAwait(false);
            return row != null;
        }

        public async Task AddUploadHistoryAsync(UploadHistoryRecord upload)
        {
            if (upload == null || string.IsNullOrWhiteSpace(upload.DeleteHash))
                return;

            await EnsureInitializedAsync().ConfigureAwait(false);

            await _db.InsertAsync(new UploadHistoryEntity
            {
                Link = upload.Link ?? string.Empty,
                DeleteHash = upload.DeleteHash,
                CreatedAt = upload.CreatedAt.ToUniversalTime().ToUnixTimeSeconds()
            }).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<UploadHistoryRecord>> GetUploadHistoryAsync(int limit)
        {
            if (limit <= 0)
                return new List<UploadHistoryRecord>();

            await EnsureInitializedAsync().ConfigureAwait(false);

            var rows = await _db.Table<UploadHistoryEntity>()
                .OrderByDescending(r => r.CreatedAt)
                .Take(limit)
                .ToListAsync()
                .ConfigureAwait(false);

            return rows.Select(r => new UploadHistoryRecord
            {
                Link = r.Link,
                DeleteHash = r.DeleteHash,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(r.CreatedAt)
            }).ToList();
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
                await InitializeAsync().ConfigureAwait(false);
        }

        private static AccountSnapshotRecord ToSnapshotRecord(AccountSnapshotEntity entity)
        {
            return new AccountSnapshotRecord
            {
                Username = entity.Username,
                Reputation = entity.Reputation,
                ReputationName = entity.ReputationName,
                CapturedAt = DateTimeOffset.FromUnixTimeSeconds(entity.CapturedAt)
            };
        }

        [Table("AccountSnapshot")]
        private sealed class AccountSnapshotEntity
        {
            [PrimaryKey, AutoIncrement]
            public int RowId { get; set; }

            [Indexed]
            public string Username { get; set; }

            public int Reputation { get; set; }

            public string ReputationName { get; set; }

            [Indexed]
            public long CapturedAt { get; set; }
        }

        [Table("NotificationSeen")]
        private sealed class NotificationSeenEntity
        {
            [PrimaryKey]
            public string Id { get; set; }

            public string Kind { get; set; }

            public long SeenAt { get; set; }
        }

        [Table("UploadHistory")]
        private sealed class UploadHistoryEntity
        {
            [PrimaryKey, AutoIncrement]
            public int RowId { get; set; }

            public string Link { get; set; }

            [Indexed]
            public string DeleteHash { get; set; }

            public long CreatedAt { get; set; }
        }
    }
}
