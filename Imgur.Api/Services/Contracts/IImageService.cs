using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Image;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IImageService: IApiService
    {
        Task<ApiResponse<UploadResponse>> UploadAsync(
            byte[] bytes,
            string fileName,
            string mimeType,
            string title = null,
            string description = null,
            string albumId = null);

        Task<ApiResponse<ImageResponse>> GetImageAsync(string imageId);
    }


}
