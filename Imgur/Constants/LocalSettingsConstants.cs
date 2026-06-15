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

        public const string AppRedirectCallback = "AppRedirectCallback";

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
            { CustomClientSecret, ""},
            { AppRedirectCallback, "ms-app://s-1-15-2-4040184719-3607775429-1870810930-2871125074-1332764820-1012269719-118339466/" }
        };
    }
}
