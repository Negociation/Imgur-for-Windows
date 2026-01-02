using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Suggest;
using Newtonsoft.Json;
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

        public async Task<ApiResponse<List<TagResponse>>> TagSearchAsync(string query, int page = 0)
        {
            //Buscar Sugestões (Max 10 Itens)...
            var urlSuggest = $"suggest?inflate=tags&q={query}&types=tags";
            var suggestTags = await GetAsync<SuggestResponse>(urlSuggest);

            if (!suggestTags.Success)
            {
                var errorResponse = new ApiResponse<List<TagResponse>>();
                errorResponse.Status = suggestTags.Status;
                errorResponse.Success = suggestTags.Success;
                return errorResponse;
            }

            var successResponse = new ApiResponse<List<TagResponse>>();
            successResponse.Data = new List<TagResponse>();
            successResponse.Success = true;
            successResponse.Status = 200;
            //Debug.WriteLine(JsonConvert.SerializeObject(suggestTags, Formatting.Indented));

            foreach (var tag in suggestTags.Data.tags)
            {
                var url = $"gallery/t/{tag.name}";
                var response = await GetAsync<TagResponse>(url);

                if (!response.Success)
                {
                    //skip
                    continue;
                }

                successResponse.Data.Add(response.Data);
            }

            return successResponse;
        }

        public async Task<ApiResponse<List<GalleryItemResponse>>> GallerySearchAsync(string query, int page = 0)
        {
            var url = $"gallery/search/{page}?q_any={query}";
            return await GetAsync<List<GalleryItemResponse>>(url);
        }
    }
}
