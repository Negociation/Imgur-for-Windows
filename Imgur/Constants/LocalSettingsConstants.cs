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


        public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
            {
                { LiveTiles, true },
                { RemindNotifications, true },
                { DataNotifications, true },
                { HDonWifi, false },
                { ThumbSize, 0 },
                { AutoPlay, false }
            };


    }
}
