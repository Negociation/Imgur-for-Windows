using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Api.Services.Models.Response.Image;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgur.Services
{
    public class AccountService
    {
        private readonly ILocalSettings _localSettings;
        private readonly IAccountService _apiService;
        private readonly IUserContext _userContext;
        private readonly AccountMapper _accountMapper;
        private readonly GalleryMapper _galleryMapper;
        private readonly CommentMapper _commentMapper;
        private readonly ImageMapper _imageMapper;

        public AccountService(
           ILocalSettings localSettings,
           IAccountService apiService,
           IUserContext userContext,
           AccountMapper accountMapper,
           GalleryMapper galleryMapper,
           CommentMapper commentMapper,
           ImageMapper imageMapper
           )
        {
            _localSettings = localSettings;
            _apiService = apiService;
            _accountMapper = accountMapper;
            _galleryMapper = galleryMapper;
            _commentMapper = commentMapper;
            _imageMapper = imageMapper;
            _userContext = userContext;
        }

        public async Task<Result<UserAccount>> GetAccountById(string id)
        {
            // Sempre busca a conta completa (cover, reputation, etc.).
            // GetFollowStatusAsync é outro endpoint e não devolve o perfil completo.
            var accountResponse = await _apiService.GetAccountAsync(id);

            if (!accountResponse.Success)
            {
                return Result<UserAccount>.Failure(
                    accountResponse.Status.ToString(),
                    ErrorType.Server);
            }

            bool? followStatus = accountResponse.Data.user_follow?.status;
            if (_userContext.IsAuthenticated && followStatus == null)
            {
                var followResponse = await _apiService.GetFollowStatusAsync(id);
                if (followResponse.Success)
                    followStatus = followResponse.Data?.user_follow?.status;
            }

            var account = _accountMapper.ToUserAccount(
                accountResponse.Data,
                followStatus
            );

            return Result<UserAccount>.Success(account);
        }

        /// <summary>
        /// Following list — stub: returns empty until API wiring is added.
        /// </summary>
        public async Task<Result<List<UserAccount>>> GetAccountFollowingAsync(string username, int page = 0)
        {
            await Task.CompletedTask;
            return Result<List<UserAccount>>.Success(new List<UserAccount>());
        }

        public async Task<Result<bool>> GetFollowStatusAsync(string username)
        {
            var response = await _apiService.GetFollowStatusAsync(username);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IReadOnlyList<UserAccount>>> SearchAccounts(string query, int page = 0)
        {
            var items = await _apiService.AccountSearchAsync(query, page);

            if (!items.Success)
            {
                return Result<IReadOnlyList<UserAccount>>.Failure(items.Status.ToString(), ErrorType.Server);
            }

            var tagListMapped = _accountMapper.ToUserAccountList(items.Data);
            return Result<IReadOnlyList<UserAccount>>.Success(tagListMapped);
        }

        public async Task<Result<List<Media>>> GetAccountFavoritesAsync(string username, int page = 0)
        {
            var response = await _apiService.GetAccountFavoritesAsync(username, page);
            if (!response.Success)
                return Result<List<Media>>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<List<Media>>.Success(_galleryMapper.ToMediaList(response.Data));
        }

        public async Task<Result<List<Comment>>> GetAccountCommentsAsync(string username, string sort = "newest", int page = 0)
        {
            var response = await _apiService.GetAccountCommentsAsync(username, sort, page);
            if (!response.Success)
                return Result<List<Comment>>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<List<Comment>>.Success(_commentMapper.ToCommentList(response.Data));
        }

        public async Task<Result<List<Media>>> GetAccountSubmissionsAsync(string username, int page = 0)
        {
            var response = await _apiService.GetAccountSubmissionsAsync(username, page);
            if (!response.Success)
                return Result<List<Media>>.Failure(response.Status.ToString(), ErrorType.Server);

            return Result<List<Media>>.Success(_galleryMapper.ToMediaList(response.Data));
        }

        /// <summary>
        /// Public = submissions; Hidden = images !InGallery; All = account images.
        /// HasMore baseado no tamanho da página bruta da API (50), não no filtro Hidden.
        /// </summary>
        public async Task<Result<AccountPostsPage>> GetAccountPostsAsync(
            string username,
            AccountPostsVisibility visibility,
            int page = 0)
        {
            const int apiPageSize = 50;

            if (visibility == AccountPostsVisibility.Public)
            {
                var response = await _apiService.GetAccountSubmissionsAsync(username, page);
                if (!response.Success)
                    return Result<AccountPostsPage>.Failure(response.Status.ToString(), ErrorType.Server);

                var items = _galleryMapper.ToMediaList(response.Data);
                return Result<AccountPostsPage>.Success(new AccountPostsPage
                {
                    Items = items ?? new List<Media>(),
                    HasMore = (response.Data?.Count ?? 0) >= apiPageSize
                });
            }

            var imagesResponse = await _apiService.GetAccountImagesAsync(username, page);
            if (!imagesResponse.Success)
                return Result<AccountPostsPage>.Failure(imagesResponse.Status.ToString(), ErrorType.Server);

            var raw = imagesResponse.Data ?? new List<ImageResponse>();
            var data = visibility == AccountPostsVisibility.Hidden
                ? raw.Where(i => !i.InGallery).ToList()
                : raw;

            return Result<AccountPostsPage>.Success(new AccountPostsPage
            {
                Items = _imageMapper.ToMediaList(data),
                HasMore = raw.Count >= apiPageSize
            });
        }

        public async Task<Result<bool>> FollowUserAsync(string username)
        {
            var response = await _apiService.FollowUserAsync(username);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UnfollowUserAsync(string username)
        {
            var response = await _apiService.UnfollowUserAsync(username);
            if (!response.Success)
                return Result<bool>.Failure(response.Status.ToString(), ErrorType.Server);
            return Result<bool>.Success(true);
        }
    }
}
