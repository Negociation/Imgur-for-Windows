using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.UI.Notifications;

namespace Imgur.Background
{
    /// <summary>
    /// Captures account reputation into imgur.db every ~60 min and refreshes lock-screen badge/status.
    /// </summary>
    public sealed class ReputationSnapshotService
    {
        private const string VaultResource = "ImgurOAuth";

        public IAsyncAction CaptureAsync()
        {
            return CaptureCoreAsync().AsAsyncAction();
        }

        internal static Task CaptureTaskAsync()
        {
            return CaptureCoreAsync();
        }

        private static async Task CaptureCoreAsync()
        {
            try
            {
                string accessToken;
                string accountName;
                if (!TryGetToken(out accessToken, out accountName))
                {
                    Debug.WriteLine("Reputation snapshot: no token");
                    return;
                }

                var localSettings = new BackgroundLocalSettings();
                var credentials = new ImgurApiCredentialsProvider(localSettings);
                var api = new Imgur.Api.Services.Actions.AccountService(credentials.ClientId);
                api.SetAccessTokenProvider(() => accessToken);

                var response = await api.GetAccountAsync("me");
                if (response == null || !response.Success || response.Data == null)
                {
                    Debug.WriteLine("Reputation snapshot: account fetch failed");
                    return;
                }

                var data = response.Data;
                string username = string.IsNullOrWhiteSpace(data.url) ? accountName : data.url;
                int reputation = data.reputation;
                string reputationName = data.reputation_name ?? string.Empty;
                long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                await BackgroundImgurDb.InsertSnapshotAsync(username, reputation, reputationName, nowUnix);
                await BackgroundImgurDb.PruneSnapshotsOlderThanAsync(
                    DateTimeOffset.UtcNow.Subtract(AccountSnapshotRetention.MaxAge).ToUnixTimeSeconds());

                var since = DateTimeOffset.UtcNow.Subtract(ReputationTrendCalculator.Window).ToUnixTimeSeconds();
                var rows = await BackgroundImgurDb.GetSnapshotsSinceAsync(username, since);
                var snapshots = rows.Select(r => new AccountSnapshotRecord
                {
                    Username = r.Username,
                    Reputation = r.Reputation,
                    ReputationName = r.ReputationName,
                    CapturedAt = DateTimeOffset.FromUnixTimeSeconds(r.CapturedAt)
                }).ToList();

                var trend = ReputationTrendCalculator.Calculate(
                    reputation, reputationName, snapshots, DateTimeOffset.UtcNow);

                UpdateBadgeAndLockStatus(trend);
                Debug.WriteLine($"Reputation snapshot: {username} rep={reputation} delta={trend.Delta} history={trend.HasHistory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reputation snapshot: " + ex.Message);
            }
        }

        private static void UpdateBadgeAndLockStatus(ReputationTrendResult trend)
        {
            try
            {
                var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                badgeXml.DocumentElement.SetAttribute(
                    "value",
                    trend.HasHistory ? Math.Abs(trend.Delta).ToString() : "0");
                BadgeUpdateManager.CreateBadgeUpdaterForApplication()
                    .Update(new BadgeNotification(badgeXml));

                string status1 = string.IsNullOrWhiteSpace(trend.ReputationName)
                    ? "Imgur"
                    : trend.ReputationName;
                string status2 = "Rep " + trend.CurrentReputation;
                string status3 = trend.HasHistory
                    ? (trend.Delta >= 0 ? "+" : "") + trend.Delta + " (24h)"
                    : "No history yet";

                // Minimal adaptive tile carrying lock-screen detailed status lines.
                string xml =
                    "<tile><visual branding='nameAndLogo'" +
                    " lockDetailedStatus1='" + Escape(status1) + "'" +
                    " lockDetailedStatus2='" + Escape(status2) + "'" +
                    " lockDetailedStatus3='" + Escape(status3) + "'>" +
                    "<binding template='TileMedium'><text hint-style='caption'>" + Escape(status2) + "</text></binding>" +
                    "<binding template='TileWide'><text hint-style='caption'>" + Escape(status2) + " · " + Escape(status3) + "</text></binding>" +
                    "</visual></tile>";

                var doc = new XmlDocument();
                doc.LoadXml(xml);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(doc));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reputation badge/lock update failed: " + ex.Message);
            }
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static bool TryGetToken(out string accessToken, out string accountName)
        {
            accessToken = null;
            accountName = null;
            try
            {
                var vault = new PasswordVault();
                var cred = vault.FindAllByResource(VaultResource)[0];
                cred.RetrievePassword();
                accountName = cred.UserName;
                long? expires;
                string refresh;
                return TokenVaultPayload.TryParse(cred.Password, out accessToken, out refresh, out expires);
            }
            catch
            {
                return false;
            }
        }
    }
}
