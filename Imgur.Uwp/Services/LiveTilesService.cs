using Imgur.Api.Services.Models.Enum;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using Imgur.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Imgur.Uwp.Services
{
    public class LiveTilesService : ILiveTilesService
    {

        private ILocalSettings _localSettings;

        private GalleryService _apiService;

        public LiveTilesService(ILocalSettings localSettings, GalleryService galleryService)
        {
            _localSettings = localSettings;
            _apiService = galleryService;
        }

        public async Task UpdateLiveTilesAsync()
        {
            if (!_localSettings.Get<bool>(LocalSettingsConstants.LiveTiles))
            {
                Debug.WriteLine("Tiles desativadas por padrão");
                ClearAllTiles();
                return;
            }

            try
            {
                // Busca o conteúdo do explorer
                var explorerContent = await _apiService.GetExplorerMedia(GallerySection.Hot, GallerySort.Viral);

                if (!explorerContent.IsSuccess)
                {
                    Debug.WriteLine("Erro na Busca");
                    return;
                }

                // Pega os 3 primeiros itens
                var topThreeItems = explorerContent.Data.Take(3).ToList();

                Debug.WriteLine("Atualizando Live Tiles");

                // Atualiza os tiles (Medium e Wide)
                await UpdateMediumAndWideTiles(topThreeItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar Live Tiles: {ex.Message}");
            }
        }


        private void ClearAllTiles()
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.Clear();
        }

        private async Task UpdateMediumAndWideTiles(List<Media> items)
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

            // Limpa notificações antigas
            tileUpdater.Clear();

            // Habilita fila de notificações (permite rotação)
            tileUpdater.EnableNotificationQueue(true);

            foreach (var item in items)
            {
                string localImagePath = await DownloadImageForTileAsync(item.CoverPlaceholder);

                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                        {
                            new AdaptiveText()
                            {
                                Text = item.Title ?? "Explorar",
                                HintStyle = AdaptiveTextStyle.Body,
                                HintWrap = true,
                                HintMaxLines = int.MaxValue
                            },
                            new AdaptiveImage()
                            {
                                Source = localImagePath ?? "ms-appx:///Assets/DefaultTile.png",
                                HintRemoveMargin = true,
                                HintCrop = AdaptiveImageCrop.Default
                            }
                        }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                        {
                            new AdaptiveText()
                            {
                                Text = item.Title ?? "Explorar",
                                HintStyle = AdaptiveTextStyle.Body,
                                HintWrap = true
                            },
                            new AdaptiveImage()
                            {
                                Source = localImagePath ?? "ms-appx:///Assets/DefaultTile.png",
                                HintRemoveMargin = true,
                                HintCrop = AdaptiveImageCrop.Default
                            }
                        }
                            }
                        }
                    }
                };

                var notification = new TileNotification(tileContent.GetXml());
                tileUpdater.Update(notification);
            }
        }

        private async Task<string> DownloadImageForTileAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return "ms-appx:///Assets/DefaultTile.png";
                }

                // Cria um nome único para o arquivo baseado na URL
                string fileName = $"tile_{GetHashString(imageUrl)}.png";

                // Pasta local temporária para tiles
                var localFolder = ApplicationData.Current.LocalFolder;
                var tileFolder = await localFolder.CreateFolderAsync("TileImages", CreationCollisionOption.OpenIfExists);

                // Verifica se a imagem já foi baixada
                var existingFile = await tileFolder.TryGetItemAsync(fileName) as StorageFile;
                if (existingFile != null)
                {
                    return $"ms-appdata:///local/TileImages/{fileName}";
                }

                // Baixa a imagem
                using (var httpClient = new HttpClient())
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                    // Salva localmente
                    var file = await tileFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(file, imageBytes);

                    return $"ms-appdata:///local/TileImages/{fileName}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao baixar imagem para tile: {ex.Message}");
                return "ms-appx:///Assets/DefaultTile.png";
            }
        }

        private string GetHashString(string text)
        {
            // Cria um hash simples da URL para nome do arquivo
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
            }
        }

        public async Task OnLiveTilesConfigChangedAsync()
        {
            await UpdateLiveTilesAsync();
        }

    }
}
