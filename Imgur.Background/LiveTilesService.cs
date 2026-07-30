using Imgur.Api.Services.Models.Enum;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Mappers;
using Imgur.Models;
using Imgur.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
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
    /// Atualiza Live Tiles. Usado no startup do app e pela Background Task.
    /// Construtor sem parâmetros: exigência de tipo público em Windows Runtime Component.
    /// </summary>
    public sealed class LiveTilesService
    {
        public IAsyncAction UpdateLiveTilesAsync()
        {
            return UpdateLiveTilesTaskAsync().AsAsyncAction();
        }

        public IAsyncAction OnLiveTilesConfigChangedAsync()
        {
            return UpdateLiveTilesTaskAsync().AsAsyncAction();
        }

        internal static Task UpdateLiveTilesTaskAsync()
        {
            return UpdateLiveTilesCoreAsync();
        }

        private static async Task UpdateLiveTilesCoreAsync()
        {
            ILocalSettings localSettings = new BackgroundLocalSettings();

            if (!localSettings.Get<bool>(LocalSettingsConstants.LiveTiles))
            {
                Debug.WriteLine("Tiles desativadas");
                ClearAllTiles();
                return;
            }

            try
            {
                var galleryService = CreateGalleryService(localSettings);
                var explorerContent = await galleryService.GetExplorerMedia(GallerySection.Hot, GallerySort.Viral);

                if (!explorerContent.IsSuccess)
                {
                    Debug.WriteLine("Erro na Busca");
                    return;
                }

                var topItems = explorerContent.Data.Take(2).ToList();
                Debug.WriteLine("Atualizando Live Tiles");
                await UpdateMediumAndWideTiles(topItems);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao atualizar Live Tiles: {ex.Message}");
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

        private static void ClearAllTiles()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

        /// <summary>
        /// Fila (máx. 5): foto → texto → foto → texto → logo do app.
        /// </summary>
        private static async Task UpdateMediumAndWideTiles(List<Media> items)
        {
            const string defaultImage = "ms-appx:///Assets/DefaultTile.png";
            const string logoMedium = "ms-appx:///Assets/Square150x150Logo.png";
            const string logoWide = "ms-appx:///Assets/Wide310x150Logo.png";

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.Clear();
            tileUpdater.EnableNotificationQueue(true);

            foreach (var item in items)
            {
                string localImagePath = await DownloadImageForTileAsync(item.CoverPlaceholder);
                var imagePath = localImagePath ?? defaultImage;

                tileUpdater.Update(CreateImageOnlyNotification(imagePath));
                tileUpdater.Update(CreateTextOnlyNotification(item.AccountId, item.Title));
            }

            tileUpdater.Update(CreateLogoNotification(logoMedium, logoWide));
        }

        private static TileNotification CreateImageOnlyNotification(string imagePath)
        {
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath,
                                HintOverlay = 0
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath,
                                HintOverlay = 0
                            }
                        }
                    }
                }
            };

            return new TileNotification(content.GetXml());
        }

        private static TileNotification CreateTextOnlyNotification(string author, string title)
        {
            var authorLine = string.IsNullOrWhiteSpace(author)
                ? "👤 Imgur"
                : "👤 " + author.Trim();
            var titleLine = string.IsNullOrWhiteSpace(title) ? "Explorar" : title.Trim();

            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = authorLine,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintAlign = AdaptiveTextAlign.Center,
                                    HintMaxLines = 1
                                },
                                new AdaptiveText()
                                {
                                    Text = titleLine,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center,
                                    HintMaxLines = 2
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = authorLine,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintAlign = AdaptiveTextAlign.Center,
                                    HintMaxLines = 1
                                },
                                new AdaptiveText()
                                {
                                    Text = titleLine,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center,
                                    HintMaxLines = 2
                                }
                            }
                        }
                    }
                }
            };

            return new TileNotification(content.GetXml());
        }

        private static TileNotification CreateLogoNotification(string logoMedium, string logoWide)
        {
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = logoMedium,
                                HintOverlay = 0
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = logoWide,
                                HintOverlay = 0
                            }
                        }
                    }
                }
            };

            return new TileNotification(content.GetXml());
        }

        private static async Task<string> DownloadImageForTileAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return "ms-appx:///Assets/DefaultTile.png";
                }

                string fileName = $"tile_{GetHashString(imageUrl)}.png";
                var localFolder = ApplicationData.Current.LocalFolder;
                var tileFolder = await localFolder.CreateFolderAsync("TileImages", CreationCollisionOption.OpenIfExists);

                var existingFile = await tileFolder.TryGetItemAsync(fileName) as StorageFile;
                if (existingFile != null)
                {
                    return $"ms-appdata:///local/TileImages/{fileName}";
                }

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
                Debug.WriteLine($"Erro ao baixar imagem para tile: {ex.Message}");
                return "ms-appx:///Assets/DefaultTile.png";
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
    }
}
