using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Enum;
using Imgur.Api.Services.Models.Request;
using Imgur.Api.Services.Models.Response;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Comment;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Api.Services.Models.Response.Notification;
using Imgur.Api.Services.Models.Response.Suggest;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
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

        public async Task<ApiResponse<bool>> MarkNotificationViewedAsync(long notificationId)
        {
            Debug.WriteLine($"[AccountService] MarkNotificationViewed {notificationId}");
            return await PutAsync<bool>($"notification/{notificationId}");
        }

        public async Task<ApiResponse<bool>> MarkNotificationsViewedAsync(IEnumerable<long> notificationIds)
        {
            if (notificationIds == null)
                return new ApiResponse<bool> { Success = false, Status = 400, Data = false };

            var ids = new List<long>();
            foreach (var id in notificationIds)
            {
                if (id > 0)
                    ids.Add(id);
            }

            if (ids.Count == 0)
                return new ApiResponse<bool> { Success = true, Status = 200, Data = true };

            // Imgur client: POST /notification with comma-separated ids in body.
            var body = new StringContent(string.Join(",", ids), Encoding.UTF8, "text/plain");
            Debug.WriteLine($"[AccountService] MarkNotificationsViewed count={ids.Count}");
            return await PostAsync<bool>("notification", body);
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

        public async Task<ApiResponse<AccountSettingsResponse>> GetAccountSettingsAsync()
        {
            Debug.WriteLine("[AccountService] GetAccountSettings me");
            return await GetAsync<AccountSettingsResponse>("account/me/settings");
        }

        public async Task<ApiResponse<bool>> UpdateAccountSettingsAsync(UpdateAccountSettingsRequest settings)
        {
            if (settings == null)
            {
                return new ApiResponse<bool> { Success = false, Status = 400, Data = false };
            }

            if (!string.IsNullOrWhiteSpace(settings.avatar) &&
                !ImgurAvatarExtensions.IsValidApiValue(settings.avatar))
            {
                Debug.WriteLine($"[AccountService] UpdateAccountSettings rejected unknown avatar: {settings.avatar}");
                return new ApiResponse<bool> { Success = false, Status = 400, Data = false };
            }

            var fields = new List<KeyValuePair<string, string>>();
            AddIfPresent(fields, "bio", settings.bio);
            AddIfPresent(fields, "username", settings.username);
            AddIfPresent(fields, "avatar", settings.avatar);
            AddIfPresent(fields, "public_images", settings.public_images);
            AddIfPresent(fields, "messaging_enabled", settings.messaging_enabled);
            AddIfPresent(fields, "album_privacy", settings.album_privacy);
            AddIfPresent(fields, "show_mature", settings.show_mature);
            AddIfPresent(fields, "newsletter_subscribed", settings.newsletter_subscribed);
            AddIfPresent(fields, "accepted_gallery_terms", settings.accepted_gallery_terms);

            if (fields.Count == 0)
            {
                return new ApiResponse<bool> { Success = false, Status = 400, Data = false };
            }

            Debug.WriteLine("[AccountService] UpdateAccountSettings me");
            var content = new FormUrlEncodedContent(fields);
            return await PutAsync<bool>("account/me/settings", content);
        }

        public async Task<ApiResponse<AvailableAvatarsResponse>> GetAvailableAvatarsAsync()
        {
            Debug.WriteLine("[AccountService] GetAvailableAvatars me");
            return await GetAsync<AvailableAvatarsResponse>("account/me/available_avatars");
        }

        public async Task<ApiResponse<AccountAvatarResponse>> GetAccountAvatarAsync()
        {
            Debug.WriteLine("[AccountService] GetAccountAvatar me");
            return await GetAsync<AccountAvatarResponse>("account/me/avatar");
        }

        private static void AddIfPresent(List<KeyValuePair<string, string>> fields, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                fields.Add(new KeyValuePair<string, string>(key, value));
        }

        public async Task<ApiResponse<bool>> BlockUserAsync(string username)
        {
            Debug.WriteLine($"[AccountService] Block: {username}");
            return await PostAsync<bool>($"account/me/block/{username}");
        }

        public async Task<ApiResponse<bool>> UnblockUserAsync(string username)
        {
            Debug.WriteLine($"[AccountService] Unblock: {username}");
            return await DeleteAsync<bool>($"account/me/block/{username}");
        }
    }
}
