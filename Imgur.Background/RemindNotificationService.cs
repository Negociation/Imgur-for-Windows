using Imgur.Api.Services.Models.Enum;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Mappers;
using Imgur.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Imgur.Background
{
    /// <summary>
    /// Toast "Top do dia": 1º item de Top+Day, só se diferente do último exibido.
    /// </summary>
    public sealed class RemindNotificationService
    {
        private const string ToastTag = "imgur_remind";
        private const string ToastGroup = "imgur";
        private const string DefaultImage = "ms-appx:///Assets/DefaultTile.png";

        public IAsyncAction TryShowAsync()
        {
            return TryShowCoreAsync().AsAsyncAction();
        }

        internal static Task TryShowTaskAsync()
        {
            return TryShowCoreAsync();
        }

        private static async Task TryShowCoreAsync()
        {
            ILocalSettings localSettings = new BackgroundLocalSettings();

            if (!localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications))
            {
                Debug.WriteLine("Remind notifications desativadas");
                return;
            }

            try
            {
                var galleryService = CreateGalleryService(localSettings);
                var result = await galleryService.GetExplorerMedia(GallerySection.Top, GalleryWindow.Day);

                if (!result.IsSuccess || result.Data == null || result.Data.Count == 0)
                {
                    Debug.WriteLine("Remind: falha ao buscar Top do dia");
                    return;
                }

                var first = result.Data.First();
                if (string.IsNullOrWhiteSpace(first.Id))
                {
                    Debug.WriteLine("Remind: item sem Id");
                    return;
                }

                var lastId = localSettings.Get<string>(LocalSettingsConstants.LastRemindNotificationMediaId);
                if (string.Equals(lastId, first.Id, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"Remind: skip, mesmo Id ({first.Id})");
                    return;
                }

                var imageUrl = !string.IsNullOrWhiteSpace(first.CoverPlaceholder)
                    ? first.CoverPlaceholder
                    : first.CoverImage;

                string localImagePath = await DownloadImageForToastAsync(imageUrl);
                ShowToast(first.Title ?? "Top of the day", localImagePath);
                localSettings.Set(LocalSettingsConstants.LastRemindNotificationMediaId, first.Id);
                Debug.WriteLine($"Remind: toast exibido para {first.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Remind notification: {ex.Message}");
            }
        }

        private static void ShowToast(string title, string imagePath)
        {
            var content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = imagePath ?? DefaultImage,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Top of the day on Imgur",
                                HintStyle = AdaptiveTextStyle.Base
                            },
                            new AdaptiveText()
                            {
                                Text = title,
                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                            }
                        }
                    }
                }
            };

            var toast = new ToastNotification(content.GetXml())
            {
                Tag = ToastTag,
                Group = ToastGroup
            };

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private static async Task<string> DownloadImageForToastAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return DefaultImage;

                // Mesma pasta/hash das tiles → reaproveita cache se já baixou
                string fileName = $"tile_{GetHashString(imageUrl)}.png";
                var tileFolder = await ApplicationData.Current.LocalFolder
                    .CreateFolderAsync("TileImages", CreationCollisionOption.OpenIfExists);

                var existingFile = await tileFolder.TryGetItemAsync(fileName) as StorageFile;
                if (existingFile != null)
                    return $"ms-appdata:///local/TileImages/{fileName}";

                using (var httpClient = new HttpClient())
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                    var file = await tileFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(file, imageBytes);
                    return $"ms-appdata:///local/TileImages/{fileName}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Remind: erro ao baixar imagem: {ex.Message}");
                return DefaultImage;
            }
        }

        private static string GetHashString(string text)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
            }
        }

        private static GalleryService CreateGalleryService(ILocalSettings localSettings)
        {
            var credentials = new ImgurApiCredentialsProvider(localSettings);
            var apiGallery = new Api.Services.Actions.GalleryService(credentials.ClientId);
            var imageMapper = new ImageMapper(localSettings);
            var galleryMapper = new GalleryMapper(localSettings, imageMapper);
            var albumMapper = new AlbumMapper(localSettings, imageMapper);
            return new GalleryService(localSettings, apiGallery, galleryMapper, albumMapper);
        }
    }
}
