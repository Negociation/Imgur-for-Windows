using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Album;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAlbumService: IApiService
    {
        Task<ApiResponse<AlbumResponse>> GetAlbumAsync(string id);

        Task<ApiResponse<CreateAlbumResponse>> CreateAlbumAsync(string title = null, string description = null, bool hidden = true);
    }
}
