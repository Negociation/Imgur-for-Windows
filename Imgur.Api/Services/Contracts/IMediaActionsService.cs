using Imgur.Api.Services.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IMediaActionsService: IApiService
    {
        Task<ApiResponse<string>> UpvoteAsync(string galleryId);
        Task<ApiResponse<string>> DownvoteAsync(string galleryId);
        Task<ApiResponse<string>> VoteAsync(string galleryId, string vote); // "up" ou "down"
        Task<ApiResponse<string>> FavoriteImageAsync(string imageId);
        Task<ApiResponse<string>> FavoriteAlbumAsync(string albumId);
    }
}
