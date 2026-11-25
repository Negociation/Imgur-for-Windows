using Imgur.Collections;
using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public partial class AccountViewModel
    {
        //-- View Model Initilization
        public async Task InitializeAsync()
        {
            if (LoadedSuccessfully) return;

            try
            {
               OnPropertyChanged("IsUserOwnPage");
               OnPropertyChanged(nameof(ShowOwnEmptyPosts));
               OnPropertyChanged(nameof(ShowGenericEmptyPosts));
               Loading = true;

                if (!string.IsNullOrWhiteSpace(UserAccountInfo?.Username) && !UserAccountInfo.IsHidden)
                {
                    var accountResult = await _accountService.GetAccountById(UserAccountInfo.Username);
                    if (accountResult.IsSuccess && accountResult.Data != null)
                    {
                        var previousFollow = UserAccountInfo.IsFollowing;
                        UserAccountInfo = accountResult.Data;
                        if (previousFollow != null && UserAccountInfo.IsFollowing == null)
                            UserAccountInfo.IsFollowing = previousFollow;
                        IsFollowing = UserAccountInfo.IsFollowing ?? false;
                        OnPropertyChanged(nameof(IsUserOwnPage));
                    }
                }

               await Task.WhenAll(
                    FavoritesCollection.LoadNextPageAsync(),
                    SubmissionsCollection.LoadNextPageAsync(),
                    CommentsCollection.LoadNextPageAsync(),
                    FollowingCollection.LoadNextPageAsync()
                );

                if (!IsUserOwnPage && UserAccountInfo.IsFollowing == null)
                {
                    var result = await _accountService.GetFollowStatusAsync(UserAccountInfo.Username);
                    if (result.IsSuccess)
                    {
                        UserAccountInfo.IsFollowing = result.Data;
                        IsFollowing = result.Data;
                    }
                }
                LoadedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountVM] InitializeAsync erro: {ex.Message}");
                LoadedSuccessfully = false;
            }
            finally
            {
                Loading = false;
            }
        }

        //Função para criação das coleções
        private void BuildCollections()
        {
            BuildFavoritesCollection();
            BuildSubmissionsCollection();
            BuildCommentsCollection();
            BuildFollowingCollection();
        }

        private void BuildFavoritesCollection()
        {
            if (FavoritesCollection != null)
                FavoritesCollection.StateChanged -= OnFavoritesStateChanged;

            CanLoadMoreFavorites = false;
            IsFavoritesEmpty = false;
            var username = UserAccountInfo?.Username;

            FavoritesCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountFavoritesAsync(username, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    foreach (var media in result.Data)
                    {
                        Debug.WriteLine(
                            $"[Favorites] page={page} Id={media?.Id} IsAlbum={media?.IsAlbum} " +
                            $"Elements={media?.Elements?.Count ?? 0} ImagesCount={media?.ImagesCount} " +
                            $"Link='{media?.Link}' CoverLink='{media?.CoverLink}' CoverImage='{media?.CoverImage}'");

                        if (media?.Elements != null)
                        {
                            for (int i = 0; i < media.Elements.Count; i++)
                            {
                                var el = media.Elements[i];
                                Debug.WriteLine($"[Favorites]   Element[{i}] Type='{el?.Type}' Link='{el?.Link}'");
                            }
                        }
                    }

                    return result.Data.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: 30,
                batchSize: 10,
                autoTriggerNextPage: false);

            FavoritesCollection.StateChanged += OnFavoritesStateChanged;
            OnPropertyChanged(nameof(FavoritesCollection));
        }

        private void OnFavoritesStateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(HasFavorites));
            CanLoadMoreFavorites = FavoritesCollection.CanLoadMorePages;
            // Secção vazia: carregou pelo menos uma vez e não há itens nem mais páginas
            if (!CanLoadMoreFavorites)
                IsFavoritesEmpty = !HasFavorites;
        }

        private void OnSubmissionsStateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(HasSubmissions));
            CanLoadMoreSubmissions = SubmissionsCollection.CanLoadMorePages;
            if (!CanLoadMoreSubmissions)
                IsSubmissionsEmpty = !HasSubmissions;
        }

        private void OnCommentsStateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(HasComments));
            CanLoadMoreComments = CommentsCollection.CanLoadMorePages;
            if (!CanLoadMoreComments)
                IsCommentsEmpty = !HasComments;
        }

        private int _hiddenImagesApiPage;

        private void BuildSubmissionsCollection()
        {
            if (SubmissionsCollection != null)
                SubmissionsCollection.StateChanged -= OnSubmissionsStateChanged;

            CanLoadMoreSubmissions = false;
            IsSubmissionsEmpty = false;
            _hiddenImagesApiPage = 0;
            var username = UserAccountInfo?.Username;
            const int apiPageSize = 50;

            SubmissionsCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    var visibility = SelectedPostsVisibility;

                    if (visibility == AccountPostsVisibility.Hidden)
                        return await FetchHiddenPostsBatchAsync(username, apiPageSize);

                    var result = await _accountService.GetAccountPostsAsync(username, visibility, page);
                    if (!result.IsSuccess || result.Data?.Items == null)
                        return new List<MediaViewModel>();

                    return result.Data.Items.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: apiPageSize,
                batchSize: 10,
                autoTriggerNextPage: false);

            SubmissionsCollection.StateChanged += OnSubmissionsStateChanged;
            OnPropertyChanged(nameof(SubmissionsCollection));
        }

        /// <summary>
        /// Hidden: agrega páginas da API até encher o batch ou acabar,
        /// para o filtro !InGallery não matar a paginação cedo.
        /// </summary>
        private async Task<List<MediaViewModel>> FetchHiddenPostsBatchAsync(string username, int targetCount)
        {
            var batch = new List<MediaViewModel>();

            while (batch.Count < targetCount)
            {
                var result = await _accountService.GetAccountPostsAsync(
                    username,
                    AccountPostsVisibility.Hidden,
                    _hiddenImagesApiPage);

                _hiddenImagesApiPage++;

                if (!result.IsSuccess || result.Data == null)
                    break;

                if (result.Data.Items != null && result.Data.Items.Count > 0)
                {
                    batch.AddRange(result.Data.Items.Select(_mediaVmFactory.GetMediaViewModel));
                }

                if (!result.Data.HasMore)
                    break;
            }

            return batch;
        }


        private void BuildCommentsCollection()
        {
            if (CommentsCollection != null)
                CommentsCollection.StateChanged -= OnCommentsStateChanged;

            CanLoadMoreComments = false;
            IsCommentsEmpty = false;
            var username = UserAccountInfo?.Username;

            CommentsCollection = _collectionFactory.Create<CommentViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountCommentsAsync(username, "newest", page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<CommentViewModel>();

                    var vms = result.Data
                        .Select(c => _commentVmFactory.GetCommentViewModel(c))
                        .ToList();

                    // Validate account payload: do items embed children (subcomments)?
                    foreach (var vm in vms.Take(8))
                    {
                        var c = vm?.Comment;
                        if (c == null) continue;
                        Debug.WriteLine(
                            $"[AccountVM] comment id={c.Id} imageId={c.ImageId} onAlbum={c.OnAlbum} children={c.Children?.Count ?? 0}");
                    }

                    return vms;
                },
                pageSize: 30,
                batchSize: 10,
                autoTriggerNextPage: false);

            CommentsCollection.StateChanged += OnCommentsStateChanged;
            OnPropertyChanged(nameof(CommentsCollection));
        }

        private void BuildFollowingCollection()
        {
            if (FollowingCollection != null)
                FollowingCollection.StateChanged -= OnFollowingStateChanged;

            IsFollowingEmpty = false;
            var username = UserAccountInfo?.Username;

            FollowingCollection = _collectionFactory.Create<AccountViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountFollowingAsync(username, page);
                    if (!result.IsSuccess || result.Data == null || result.Data.Count == 0)
                        return new List<AccountViewModel>();

                    return result.Data.Select(_accountVmFactory.GetAccountViewModel).ToList();
                },
                pageSize: 30,
                batchSize: 10,
                autoTriggerNextPage: false);

            FollowingCollection.StateChanged += OnFollowingStateChanged;
            OnPropertyChanged(nameof(FollowingCollection));
        }

        private void OnFollowingStateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(HasFollowing));
            if (!FollowingCollection.CanLoadMorePages)
                IsFollowingEmpty = !HasFollowing;
        }
    }
}
