using Imgur.Contracts;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// Pins a single Start secondary tile for the app mark (reuses primary logo assets).
    /// </summary>
    public sealed class StartTilePinService : IStartTilePinService
    {
        public const string TileId = "Imgur.StartMark";

        private static readonly Uri MediumLogo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
        private static readonly Uri SmallLogo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

        public async Task<bool> PinAsync(bool transparent = false)
        {
            Color plate = transparent
                ? Colors.Transparent
                : new UISettings().GetColorValue(UIColorType.Accent);

            try
            {
                if (SecondaryTile.Exists(TileId))
                {
                    var existing = new SecondaryTile(TileId)
                    {
                        DisplayName = "Imgur",
                        Arguments = "app"
                    };
                    ApplyChrome(existing, plate, transparent);
                    return await existing.UpdateAsync();
                }

                var tile = new SecondaryTile(
                    TileId,
                    "Imgur",
                    "app",
                    MediumLogo,
                    TileSize.Square150x150)
                {
                    DisplayName = "Imgur"
                };
                ApplyChrome(tile, plate, transparent);
                return await tile.RequestCreateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[StartTilePinService] Pin failed: " + ex.Message);
                return false;
            }
        }

        private static void ApplyChrome(SecondaryTile tile, Color plate, bool transparent)
        {
            tile.VisualElements.Square150x150Logo = MediumLogo;
            tile.VisualElements.Square71x71Logo = SmallLogo;
            tile.VisualElements.BackgroundColor = plate;
            tile.VisualElements.ShowNameOnSquare150x150Logo = !transparent;
            tile.VisualElements.ForegroundText = transparent || Luminance(plate) > 160
                ? ForegroundText.Dark
                : ForegroundText.Light;
        }

        private static double Luminance(Color color)
        {
            return (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
        }
    }
}
