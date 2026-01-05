using System.Collections.Generic;

namespace Imgur.Constants
{
    public static class LocalSettingsConstants
    {


        public const string LiveTiles = "LiveTiles";

        public const string RemindNotifications = "RemindNotifications";

        public const string DataNotifications = "DataNotifications";

        public const string HDonWifi = "HDonWifi";

        public const string ThumbSize = "ThumbSize";

        public const string AutoPlay = "AutoPlay";

        public const string ClientId = "ClientId";

        public const string ClientSecret = "ClientSecret";

        public const string CustomClientId = "CustomClientId";

        public const string CustomClientSecret = "CustomClientSecret";

        public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
        {
            { LiveTiles, true },
            { RemindNotifications, true },
            { DataNotifications, true },
            { HDonWifi, false },
            { ThumbSize, 0 },
            { AutoPlay, false },
            { ClientId, "b6c4abc4061d423" },
            { ClientSecret, "1ccc6187d2e64baaefcf49487cc1d948cfa6484e"},
            { CustomClientId, "" },
            { CustomClientSecret, ""}
        };
    }
}
