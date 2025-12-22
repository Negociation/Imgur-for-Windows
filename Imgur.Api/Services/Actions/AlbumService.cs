using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
               $"gallery/album/{id}");
        }
    }
}
