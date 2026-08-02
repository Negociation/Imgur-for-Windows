using Imgur.Contracts;
using Imgur.Models;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Imgur.Uwp.Services
{
    public sealed class SqliteViewHistoryStore : IViewHistoryStore
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

                // PCLRaw 1.1.x (UAP 15063)
                SQLitePCL.Batteries_V2.Init();

                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "view_history.db");
                _db = new SQLiteAsyncConnection(path);
                await _db.CreateTableAsync<ViewedMediaEntity>().ConfigureAwait(false);
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewHistory] Initialize failed: {ex.Message}");
                throw;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<bool> HasBeenViewedAsync(string mediaId, string accountKey)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return false;

            await EnsureInitializedAsync().ConfigureAwait(false);

            var id = BuildId(mediaId, accountKey);
            var row = await _db.FindAsync<ViewedMediaEntity>(id).ConfigureAwait(false);
            return row != null;
        }

        public async Task<bool> TryRecordFirstViewAsync(string mediaId, string accountKey)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return false;

            await EnsureInitializedAsync().ConfigureAwait(false);

            var entity = new ViewedMediaEntity
            {
                Id = BuildId(mediaId, accountKey),
                MediaId = mediaId,
                AccountKey = NormalizeAccountKey(accountKey),
                ViewedAt = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
            };

            try
            {
                // InsertAsync lança se PK existir; InsertOrIgnore não existe em todas as versões —
                // usamos INSERT OR IGNORE via SQL.
                var changes = await _db.ExecuteAsync(
                    "INSERT OR IGNORE INTO viewed_media (Id, MediaId, AccountKey, ViewedAt) VALUES (?, ?, ?, ?)",
                    entity.Id, entity.MediaId, entity.AccountKey, entity.ViewedAt).ConfigureAwait(false);

                return changes > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewHistory] TryRecordFirstView failed: {ex.Message}");
                return false;
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
                await InitializeAsync().ConfigureAwait(false);
        }

        private static string NormalizeAccountKey(string accountKey) =>
            string.IsNullOrWhiteSpace(accountKey) ? string.Empty : accountKey.Trim();

        private static string BuildId(string mediaId, string accountKey) =>
            NormalizeAccountKey(accountKey) + "|" + mediaId;

        [Table("viewed_media")]
        private sealed class ViewedMediaEntity
        {
            [PrimaryKey]
            public string Id { get; set; }

            [Indexed]
            public string MediaId { get; set; }

            public string AccountKey { get; set; }

            public long ViewedAt { get; set; }
        }
    }
}
