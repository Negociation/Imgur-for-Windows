using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class GalleryService : ApiService, IGalleryService
    {
        public GalleryService(string clientId) : base(clientId)
        {
        }

        public async Task<ApiResponse<List<GalleryItemResponse>>> GetGalleryAsync(
            GallerySection section = GallerySection.Hot,
            GallerySort sort = GallerySort.Viral,
            GalleryWindow window = GalleryWindow.Day,
            int page = 0,
            bool showViral = false,
            bool showMature = false,
            bool albumPreviews = false)
        {
            var query = $"?showViral={showViral.ToString().ToLower()}" +
                        $"&mature={showMature.ToString().ToLower()}" +
                        $"&album_previews={albumPreviews.ToString().ToLower()}";

            var url = $"gallery/{section.ToString().ToLower()}/{sort.ToString().ToLower()}/{page}/{window}{query}";

            Debug.WriteLine("Url Alvo:" + url);

            return await GetAsync<List<GalleryItemResponse>>(url);
        }

        public async Task<ApiResponse<List<GalleryItemResponse>>> GetRandomAsync(int page = 0)
        {
            var url = $"gallery/random/";
            return await GetAsync<List<GalleryItemResponse>>(url);
        }


        public async Task<ApiResponse<GalleryItemResponse>> GetGalleryAlbumAsync(string id)
        {
            return await GetAsync<GalleryItemResponse>(
               $"gallery/album/{id}");
        }

        public async Task<ApiResponse<TagResponse>> GetTagAsync(string name,GallerySort sort = GallerySort.Viral, GalleryWindow window = GalleryWindow.Day, int page = 0)
        {
            var url = $"gallery/t/{name.ToLower()}/{sort.ToString().ToLower()}/{page}/{window}";
            return await GetAsync<TagResponse>(url);
        }

    }
}
