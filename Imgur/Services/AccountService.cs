using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Common;
using Imgur.Api.Services.Models.Response.Account;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Mappers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public AccountService(
           ILocalSettings localSettings,
           IAccountService apiService,
           IUserContext userContext,
           AccountMapper accountMapper,
           GalleryMapper galleryMapper,
           CommentMapper commentMapper
           )
        {
            _localSettings = localSettings;
            _apiService = apiService;
            _accountMapper = accountMapper;
            _galleryMapper = galleryMapper;
            _commentMapper = commentMapper;
            _userContext = userContext;
        }

        public async Task<Result<UserAccount>> GetAccountById(string id)
        {
            ApiResponse<AccountResponse> accountResponse;

            if (_userContext.IsAuthenticated)
            {
                accountResponse = await _apiService.GetFollowStatusAsync(id);
            }
            else
            {
                accountResponse = await _apiService.GetAccountAsync(id);
            }

            if (!accountResponse.Success)
            {
                return Result<UserAccount>.Failure(
                    accountResponse.Status.ToString(),
                    ErrorType.Server);
            }

            var account = _accountMapper.ToUserAccount(
                accountResponse.Data,
                accountResponse.Data.user_follow?.status
            );

            return Result<UserAccount>.Success(account);
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
            ApiResponse<List<AccountResponse>> items = null;
            items = await _apiService.AccountSearchAsync(query, page);

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
