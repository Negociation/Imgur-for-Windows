using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Comment;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Contracts
{
    public interface IAccountService: IApiService
    {
        Task<ApiResponse<AccountResponse>> GetAccountAsync(string id);

        Task<ApiResponse<List<GalleryItemResponse>>> GetAccountFavoritesAsync(string username, int page = 0);
        Task<ApiResponse<List<CommentResponse>>> GetAccountCommentsAsync(string username, string sort = "newest", int page = 0);
        Task<ApiResponse<List<GalleryItemResponse>>> GetAccountSubmissionsAsync(string username, int page = 0);

        Task<ApiResponse<List<AccountResponse>>> AccountSearchAsync(string query, int page = 0);

    }
}
