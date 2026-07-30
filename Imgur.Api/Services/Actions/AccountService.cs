using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Comment;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Api.Services.Models.Response.Notification;
using Imgur.Api.Services.Models.Response.Suggest;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Imgur.Api.Services.Actions
{
    public class AccountService : ApiService, IAccountService
    {
        public AccountService(string clientId) : base(clientId)
        {
        }

        public async Task<ApiResponse<AccountResponse>> GetAccountAsync(string id)
        {
            return await GetAsync<AccountResponse>(
               $"account/{id}");
        }

        public async Task<ApiResponse<List<GalleryItemResponse>>> GetAccountFavoritesAsync(string username, int page = 0)
        {
            Debug.WriteLine($"[AccountService] GetFavorites: {username} page:{page}");
            return await GetAsync<List<GalleryItemResponse>>($"account/{username}/favorites/{page}");
        }

        public async Task<ApiResponse<List<CommentResponse>>> GetAccountCommentsAsync(
            string username, string sort = "newest", int page = 0)
        {
            Debug.WriteLine($"[AccountService] GetComments: {username} page:{page}");
            return await GetAsync<List<CommentResponse>>($"account/{username}/comments/{sort}/{page}");
        }

        public async Task<ApiResponse<List<GalleryItemResponse>>> GetAccountSubmissionsAsync(
            string username, int page = 0)
        {
            Debug.WriteLine($"[AccountService] GetSubmissions: {username} page:{page}");
            return await GetAsync<List<GalleryItemResponse>>($"account/{username}/submissions/{page}");
        }

        public async Task<ApiResponse<List<ImageResponse>>> GetAccountImagesAsync(
            string username, int page = 0)
        {
            Debug.WriteLine($"[AccountService] GetImages: {username} page:{page}");
            return await GetAsync<List<ImageResponse>>($"account/{username}/images/{page}");
        }

        public async Task<ApiResponse<AccountNotificationsData>> GetNotificationsAsync(bool onlyNew = true)
        {
            var newParam = onlyNew ? "true" : "false";
            Debug.WriteLine($"[AccountService] GetNotifications new:{newParam}");
            return await GetAsync<AccountNotificationsData>($"account/me/notifications?new={newParam}");
        }

        public async Task<ApiResponse<List<AccountResponse>>> AccountSearchAsync(string query, int page = 0)
        {
            //Buscar Sugestões (Max 10 Itens)...
            var urlSuggest = $"suggest?inflate=users&q={query}&types=users";
            var suggestTags = await GetAsync<SuggestResponse>(urlSuggest);

            if (!suggestTags.Success)
            {
                var errorResponse = new ApiResponse<List<AccountResponse>>();
                errorResponse.Status = suggestTags.Status;
                errorResponse.Success = suggestTags.Success;
                return errorResponse;
            }

            var successResponse = new ApiResponse<List<AccountResponse>>();
            successResponse.Data = new List<AccountResponse>();
            successResponse.Success = true;
            successResponse.Status = 200;

            foreach (var user in suggestTags.Data.users)
            {
                var response = await GetAccountAsync(user.url);

                if (!response.Success)
                {
                    continue;
                }

                successResponse.Data.Add(response.Data);
            }

            return successResponse;
        }

        public async Task<ApiResponse<bool>> FollowUserAsync(string username)
        {
            Debug.WriteLine($"[AccountService] Follow: {username}");
            return await PostAsync<bool>($"account/{username}/follow");
        }

        public async Task<ApiResponse<bool>> UnfollowUserAsync(string username)
        {
            Debug.WriteLine($"[AccountService] Unfollow: {username}");
            return await DeleteAsync<bool>($"account/{username}/follow");
        }

        public async Task<ApiResponse<AccountResponse>> GetFollowStatusAsync(string username)
        {
            return await GetAsync<AccountResponse>($"account/{username}/follow");
        }
    }
}
