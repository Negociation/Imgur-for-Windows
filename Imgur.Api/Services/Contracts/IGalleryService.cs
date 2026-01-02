using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IGalleryService
    {
        Task<ApiResponse<List<GalleryItemResponse>>> GetGalleryAsync(
           GallerySection section = GallerySection.Hot,
            GallerySort sort = GallerySort.Viral,
            GalleryWindow window = GalleryWindow.Day,
            int page = 0,
            bool showViral = false,
            bool showMature = false,
            bool albumPreviews = false);

        Task<ApiResponse<GalleryItemResponse>> GetGalleryAlbumAsync(string id);

        Task<ApiResponse<List<GalleryItemResponse>>> GetRandomAsync(int page = 0);

        Task<ApiResponse<TagResponse>> GetTagAsync(
            string name,
            GallerySort sort = GallerySort.Viral,
            GalleryWindow window = GalleryWindow.Day,
            int page = 0
        );

        Task<ApiResponse<List<GalleryItemResponse>>> GallerySearchAsync(string query, int page = 0);

        Task<ApiResponse<List<TagResponse>>> TagSearchAsync(string query, int page = 0);

    }
}
