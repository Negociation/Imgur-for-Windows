using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Imgur.Background
{
    /// <summary>
    /// Shared LocalFolder imgur.db access for background tasks (same file as UI SqliteImgurStore).
    /// </summary>
    internal static class BackgroundImgurDb
    {
        private static SQLiteAsyncConnection _db;
        private static bool _initialized;

        public static async Task EnsureInitializedAsync()
        {
            if (_initialized)
                return;

            try
            {
                SQLitePCL.Batteries_V2.Init();
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "imgur.db");
                _db = new SQLiteAsyncConnection(path);
                await _db.ExecuteAsync("PRAGMA journal_mode=WAL;");
                await _db.CreateTableAsync<AccountSnapshotEntity>();
                await _db.CreateTableAsync<NotificationSeenEntity>();
                await _db.CreateTableAsync<UploadHistoryEntity>();
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[BackgroundImgurDb] init failed: " + ex.Message);
                throw;
            }
        }

        public static async Task InsertSnapshotAsync(string username, int reputation, string reputationName, long capturedAtUnix)
        {
            await EnsureInitializedAsync();
            await _db.InsertAsync(new AccountSnapshotEntity
            {
                Username = username,
                Reputation = reputation,
                ReputationName = reputationName ?? string.Empty,
                CapturedAt = capturedAtUnix
            });
        }

        public static async Task<List<AccountSnapshotEntity>> GetSnapshotsSinceAsync(string username, long sinceUnix)
        {
            await EnsureInitializedAsync();
            string user = username;
            return await _db.Table<AccountSnapshotEntity>()
                .Where(r => r.Username == user && r.CapturedAt >= sinceUnix)
                .OrderBy(r => r.CapturedAt)
                .ToListAsync();
        }

        public static async Task PruneSnapshotsOlderThanAsync(long cutoffUnix)
        {
            await EnsureInitializedAsync();
            await _db.ExecuteAsync("DELETE FROM AccountSnapshot WHERE CapturedAt < ?", cutoffUnix);
        }

        public static async Task<bool> TryMarkNotificationSeenAsync(string id, string kind, long seenAtUnix)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            await EnsureInitializedAsync();
            try
            {
                var changes = await _db.ExecuteAsync(
                    "INSERT OR IGNORE INTO NotificationSeen (Id, Kind, SeenAt) VALUES (?, ?, ?)",
                    id, kind ?? string.Empty, seenAtUnix);
                return changes > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[BackgroundImgurDb] mark seen failed: " + ex.Message);
                return false;
            }
        }

        [Table("AccountSnapshot")]
        public sealed class AccountSnapshotEntity
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
