using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Comment;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Api.Services.Models.Response.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAccountService: IApiService
    {
        Task<ApiResponse<AccountResponse>> GetAccountAsync(string id);

        Task<ApiResponse<List<GalleryItemResponse>>> GetAccountFavoritesAsync(string username, int page = 0);
        Task<ApiResponse<List<CommentResponse>>> GetAccountCommentsAsync(string username, string sort = "newest", int page = 0);
        Task<ApiResponse<List<GalleryItemResponse>>> GetAccountSubmissionsAsync(string username, int page = 0);

        Task<ApiResponse<List<ImageResponse>>> GetAccountImagesAsync(string username, int page = 0);

        /// <param name="onlyNew">true = só não vistas (default da API).</param>
        Task<ApiResponse<AccountNotificationsData>> GetNotificationsAsync(bool onlyNew = true);

        Task<ApiResponse<List<AccountResponse>>> AccountSearchAsync(string query, int page = 0);

        Task<ApiResponse<bool>> FollowUserAsync(string username);
        Task<ApiResponse<bool>> UnfollowUserAsync(string username);

        Task<ApiResponse<AccountResponse>> GetFollowStatusAsync(string username);
    }
}
