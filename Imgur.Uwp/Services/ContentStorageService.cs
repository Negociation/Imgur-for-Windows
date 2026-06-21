namespace Imgur.Uwp.Services
{
    using Imgur.Api.Services.Contracts;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.AccessCache;
    using Windows.Storage.Streams;

    public class ContentStorageService : IContentStorageService
    {
        private readonly HttpClient _httpClient;

        public ContentStorageService(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> SaveAsync(
            Uri contentUrl,
            string folderToken,
            string fileName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (contentUrl == null)
                throw new ArgumentNullException(nameof(contentUrl));

            if (string.IsNullOrWhiteSpace(folderToken))
                throw new ArgumentNullException(nameof(folderToken));

            var folder = await StorageApplicationPermissions
                .FutureAccessList
                .GetFolderAsync(folderToken);

            var file = await folder.CreateFileAsync(
                fileName,
                CreationCollisionOption.ReplaceExisting);

            using (var response = await _httpClient.GetAsync(
                contentUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                using (var inputStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                using (var outputStream = fileStream.GetOutputStreamAt(0))
                {
                    await inputStream.CopyToAsync(outputStream.AsStreamForWrite());
                    await outputStream.FlushAsync();
                }
            }

            // retorna o token (ou você pode retornar file.Name se quiser)
            return folderToken;
        }
    }
}