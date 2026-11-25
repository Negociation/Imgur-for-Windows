using System.Collections.Generic;
using Imgur.Models;

namespace Imgur.Constants
{
    public static class LocalSettingsConstants
    {


        public const string LiveTiles = "LiveTiles";

        public const string RemindNotifications = "RemindNotifications";

        public const string AccountNotifications = "AccountNotifications";

        public const string DataNotifications = "DataNotifications";

        public const string HDonWifi = "HDonWifi";

        public const string ThumbSize = "ThumbSize";

        public const string AutoPlay = "AutoPlay";

        public const string ClientId = "ClientId";

        public const string ClientSecret = "ClientSecret";

        public const string CustomClientId = "CustomClientId";

        public const string CustomClientSecret = "CustomClientSecret";

        public const string AppRedirectCallback = "AppRedirectCallback";

        public const string LastRemindNotificationMediaId = "LastRemindNotificationMediaId";

        public const string LastAccountNotificationId = "LastAccountNotificationId";

        /// <summary>True while the UWP process is in the foreground (for background toast suppression).</summary>
        public const string AppIsForeground = "AppIsForeground";

        /// <summary>Selected UI language (<see cref="AppLanguage"/> as int). Default System.</summary>
        public const string SelectedLanguage = "SelectedLanguage";

        public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
        {
            { LiveTiles, true },
            { RemindNotifications, true },
            { AccountNotifications, true },
            { DataNotifications, true },
            { HDonWifi, false },
            { ThumbSize, 0 },
            { AutoPlay, false },
            { ClientId, "b6c4abc4061d423" },
            { ClientSecret, "1ccc6187d2e64baaefcf49487cc1d948cfa6484e"},
            { CustomClientId, "" },
            { CustomClientSecret, ""},
            { AppRedirectCallback, "" },
            { LastRemindNotificationMediaId, "" },
            { LastAccountNotificationId, "" },
            { AppIsForeground, false },
            { SelectedLanguage, (int)AppLanguage.System }
        };
    }
}
