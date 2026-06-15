using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Album;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class AlbumService : ApiService, IAlbumService
    {
        public AlbumService(string clientId) : base(clientId)
        {
        }
        
        public async Task<ApiResponse<AlbumResponse>> GetAlbumAsync(string id)
        {
            return await GetAsync<AlbumResponse>(
               $"album/{id}");
        }

        public async Task<ApiResponse<CreateAlbumResponse>> CreateAlbumAsync(string title = null, string description = null, bool hidden = true)
        {
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("title",       title       ?? string.Empty),
                new KeyValuePair<string, string>("description", description ?? string.Empty),
                new KeyValuePair<string, string>("privacy",     hidden ? "hidden" : "public" ),
            });

            return await PostAsync<CreateAlbumResponse>("album", body);
        }
    }
}
