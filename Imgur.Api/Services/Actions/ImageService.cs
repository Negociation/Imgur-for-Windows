using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Image;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    // ImageService.cs
    public class ImageService : ApiService, IImageService
    {
        public ImageService(string clientId) : base(clientId) { }
        public async Task<ApiResponse<UploadResponse>> UploadAsync(
            byte[] bytes,
            string fileName,
            string mimeType,
            string title = null,
            string description = null,
            string albumId = null)
        {
            Debug.WriteLine($"[ImageUploadService] Bytes: {bytes?.Length}");
            Debug.WriteLine($"[ImageUploadService] FileName: {fileName}");
            Debug.WriteLine($"[ImageUploadService] AlbumId: {albumId}");
            Debug.WriteLine($"[ImageUploadService] Title: {title}");

            if (bytes == null || bytes.Length == 0)
                return new ApiResponse<UploadResponse> { Success = false, Status = 0 };

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(bytes), "image", fileName);

            if (!string.IsNullOrEmpty(title))
                content.Add(new StringContent(title), "title");

            if (!string.IsNullOrEmpty(description))
                content.Add(new StringContent(description), "description");

            if (!string.IsNullOrEmpty(albumId))
                content.Add(new StringContent(albumId), "album");

            return await PostAsync<UploadResponse>("image", content);
        }

        public async Task<ApiResponse<ImageResponse>> GetImageAsync(string imageId)
        {
            Debug.WriteLine($"[ImageService] GetImage: {imageId}");
            return await GetAsync<ImageResponse>($"image/{imageId}");
        }
    }
}
