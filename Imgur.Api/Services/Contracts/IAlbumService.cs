using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAlbumService
    {
        Task<ApiResponse<AlbumResponse>> GetAlbumAsync(string id);
    }
}
