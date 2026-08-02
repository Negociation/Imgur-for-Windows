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
    public class AccountViewModel : Observable
    {

        //-- Loading Content Flag Property
        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged("Loading");
                OnPropertyChanged(nameof(ShowErrorPage));
            }
        }

        //-- Loading Success State Flag Property
        private bool _loadedSuccessfully;

        public bool LoadedSuccessfully
        {
            get { return _loadedSuccessfully; }
            set
            {
                _loadedSuccessfully = value;
                OnPropertyChanged("LoadedSuccessfully");
                OnPropertyChanged(nameof(ShowErrorPage));
            }
        }

        public bool ShowErrorPage => !Loading && !LoadedSuccessfully;

        //-- Thumbnail Config Size
        public int ThumbSize => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);

        //-- User Account Profile
        private UserAccount _userAccountInfo;

        public UserAccount UserAccountInfo
        {
            get { return _userAccountInfo; }
            set
            {
                _userAccountInfo = value;
                OnPropertyChanged("UserAccountInfo");
            }
        }

        public IIncrementalCollection<MediaViewModel> FavoritesCollection { get; private set; }
        public IIncrementalCollection<MediaViewModel> SubmissionsCollection { get; private set; }
        public IIncrementalCollection<CommentViewModel> CommentsCollection { get; private set; }
        public IIncrementalCollection<AccountViewModel> FollowingCollection { get; private set; }

        public bool HasFavorites => (FavoritesCollection?.Count ?? 0) > 0;
        public bool HasSubmissions => (SubmissionsCollection?.Count ?? 0) > 0;
        public bool HasComments => (CommentsCollection?.Count ?? 0) > 0;
        public bool HasFollowing => (FollowingCollection?.Count ?? 0) > 0;

        //-- Flag de carregamento de nova página (footer ProgressBar)
        private bool _isLoadingNewPage;

        public bool IsLoadingNewPage
        {
            get { return _isLoadingNewPage; }
            set
            {
                _isLoadingNewPage = value;
                OnPropertyChanged(nameof(IsLoadingNewPage));
                OnPropertyChanged(nameof(ShowLoadMoreSubmissionsButton));
                OnPropertyChanged(nameof(ShowLoadMoreFavoritesButton));
                OnPropertyChanged(nameof(ShowLoadMoreCommentsButton));
            }
        }

        //-- CanLoadMore por coleção (atualizado via StateChanged quando buffer esgota)
        private bool _canLoadMoreSubmissions;

        public bool CanLoadMoreSubmissions
        {
            get { return _canLoadMoreSubmissions; }
            set
            {
                _canLoadMoreSubmissions = value;
                OnPropertyChanged(nameof(CanLoadMoreSubmissions));
                OnPropertyChanged(nameof(ShowLoadMoreSubmissionsButton));
            }
        }

        private bool _canLoadMoreFavorites;

        public bool CanLoadMoreFavorites
        {
            get { return _canLoadMoreFavorites; }
            set
            {
                _canLoadMoreFavorites = value;
                OnPropertyChanged(nameof(CanLoadMoreFavorites));
                OnPropertyChanged(nameof(ShowLoadMoreFavoritesButton));
            }
        }

        private bool _canLoadMoreComments;

        public bool CanLoadMoreComments
        {
            get { return _canLoadMoreComments; }
            set
            {
                _canLoadMoreComments = value;
                OnPropertyChanged(nameof(CanLoadMoreComments));
                OnPropertyChanged(nameof(ShowLoadMoreCommentsButton));
            }
        }

        //-- Visibilidade efetiva do botão: só aparece se há mais páginas E não está carregando
        public bool ShowLoadMoreSubmissionsButton => CanLoadMoreSubmissions && !IsLoadingNewPage;
        public bool ShowLoadMoreFavoritesButton => CanLoadMoreFavorites && !IsLoadingNewPage;
        public bool ShowLoadMoreCommentsButton => CanLoadMoreComments && !IsLoadingNewPage;

        //-- Estado vazio por secção: coleção carregou e não retornou nenhum item
        private bool _isSubmissionsEmpty;
        public bool IsSubmissionsEmpty
        {
            get { return _isSubmissionsEmpty; }
            private set
            {
                _isSubmissionsEmpty = value;
                OnPropertyChanged(nameof(IsSubmissionsEmpty));
                OnPropertyChanged(nameof(ShowOwnEmptyPosts));
                OnPropertyChanged(nameof(ShowGenericEmptyPosts));
            }
        }

        /// <summary>Empty state do próprio perfil (CTA New Post).</summary>
        public bool ShowOwnEmptyPosts => IsUserOwnPage && IsSubmissionsEmpty;

        /// <summary>Empty state genérico ao ver posts de outro usuário.</summary>
        public bool ShowGenericEmptyPosts => !IsUserOwnPage && IsSubmissionsEmpty;

        private bool _isFavoritesEmpty;
        public bool IsFavoritesEmpty
        {
            get { return _isFavoritesEmpty; }
            private set { _isFavoritesEmpty = value; OnPropertyChanged(nameof(IsFavoritesEmpty)); }
        }

        private bool _isCommentsEmpty;
        public bool IsCommentsEmpty
        {
            get { return _isCommentsEmpty; }
            private set { _isCommentsEmpty = value; OnPropertyChanged(nameof(IsCommentsEmpty)); }
        }

        private bool _isFollowingEmpty;
        public bool IsFollowingEmpty
        {
            get { return _isFollowingEmpty; }
            private set { _isFollowingEmpty = value; OnPropertyChanged(nameof(IsFollowingEmpty)); }
        }

        //-- Scroll to Top Flag
        private bool _canScrollToTop;

        public bool CanScrollToTop
        {
            get { return _canScrollToTop; }
            set
            {
                _canScrollToTop = value;
                OnPropertyChanged(nameof(CanScrollToTop));
            }
        }

        //-- Estado de Follow do perfil visualizado
        private bool _isFollowing;

        public bool IsFollowing
        {
            get { return _isFollowing; }
            set
            {
                _isFollowing = value;
                OnPropertyChanged(nameof(IsFollowing));
            }
        }

        //-- Usuario Atual Autenticado
        public bool IsAuthenticated => _userContext.IsAuthenticated;

        public bool IsUserOwnPage => IsAuthenticated && _userContext.CurrentUser?.Username == _userAccountInfo.Username;

        //-- Filtro Public | Hidden | All (só no próprio perfil)
        public System.Collections.ObjectModel.ObservableCollection<AccountPostsVisibility> PostsVisibilityOptions { get; } =
            new System.Collections.ObjectModel.ObservableCollection<AccountPostsVisibility>
            {
                AccountPostsVisibility.Public,
                AccountPostsVisibility.Hidden,
                AccountPostsVisibility.All
            };

        private int _selectedPostsVisibilityIndex;

        public int SelectedPostsVisibilityIndex
        {
            get { return _selectedPostsVisibilityIndex; }
            set
            {
                if (_selectedPostsVisibilityIndex == value) return;
                _selectedPostsVisibilityIndex = value;
                OnPropertyChanged(nameof(SelectedPostsVisibilityIndex));
                OnPropertyChanged(nameof(SelectedPostsVisibility));
                OnPropertyChanged(nameof(PostsVisibilityIconGlyph));
            }
        }

        public AccountPostsVisibility SelectedPostsVisibility =>
            IsUserOwnPage && _selectedPostsVisibilityIndex >= 0 && _selectedPostsVisibilityIndex < PostsVisibilityOptions.Count
                ? PostsVisibilityOptions[_selectedPostsVisibilityIndex]
                : AccountPostsVisibility.Public;

        /// <summary>
        /// Public = Contact (avatar), Hidden = RedEye, All = Picture (Segoe MDL2).
        /// </summary>
        public string PostsVisibilityIconGlyph
        {
            get
            {
                switch (SelectedPostsVisibility)
                {
                    case AccountPostsVisibility.Hidden:
                        return "\uE7B3"; // RedEye
                    case AccountPostsVisibility.All:
                        return "\uE8B9"; // Picture
                    default:
                        return "\uE77B"; // Contact / avatar
                }
            }
        }

        //***************************************************************
        // Services 
        //***************************************************************

        //-- Service para Dialogos 
        private readonly IDialogService _dialogService;

        //-- UserContext
        private readonly IUserContext _userContext;

        //-- Serviço de Navegação
        private readonly INavigator _navigator;

        //-- Factory para Collections Paginadas
        private readonly IIncrementalCollectionFactory _collectionFactory;

        //-- Account Service
        private readonly Imgur.Services.AccountService _accountService;

        //-- Media ViewModel Factory
        private readonly IMediaVmFactory _mediaVmFactory;

        //-- Comment ViewModel Factory
        private readonly ICommentVmFactory _commentVmFactory;

        //-- Servico de Settings do App
        private readonly ILocalSettings _localSettings;

        //-- Serviço de Notificações In-App
        private readonly IAppNotificationService _appNotification;
        private readonly IUriLauncher _uriLauncher;
        private readonly IAccountVmFactory _accountVmFactory;

        public AccountViewModel(
            UserAccount account,
            IUserContext userContext,
            IDialogService dialogService,
            INavigator navigator,
            ILocalSettings localSettings,
            IIncrementalCollectionFactory collectionFactory,
            Imgur.Services.AccountService accountService,
            IMediaVmFactory mediaVmFactory,
            ICommentVmFactory commentVmFactory,
            IAppNotificationService appNotification,
            IUriLauncher uriLauncher,
            IAccountVmFactory accountVmFactory
            )
        {
            this.UserAccountInfo = account;
            this._userContext = userContext;
            this._dialogService = dialogService;
            this._navigator = navigator;
            this._collectionFactory = collectionFactory;
            this._accountService = accountService;
            this._mediaVmFactory = mediaVmFactory;
            this._commentVmFactory = commentVmFactory;
            this._localSettings = localSettings;
            this._appNotification = appNotification;
            this._uriLauncher = uriLauncher;
            this._accountVmFactory = accountVmFactory;

            BuildCollections();
        }

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
                    
                    return result.Data
                        .Select(c => _commentVmFactory.GetCommentViewModel(c))
                        .ToList();
                        

                    return new List<CommentViewModel>();
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

        //-- Command para recarregar Posts (Submissions)
        private ICommand _reloadSubmissionsCommand;

        public ICommand ReloadSubmissionsCommand
        {
            get
            {
                if (_reloadSubmissionsCommand == null)
                {
                    _reloadSubmissionsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildSubmissionsCollection();
                            await SubmissionsCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadSubmissions: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadSubmissionsCommand;
            }
        }

        //-- Command para recarregar Favoritos
        private ICommand _reloadFavoritesCommand;

        public ICommand ReloadFavoritesCommand
        {
            get
            {
                if (_reloadFavoritesCommand == null)
                {
                    _reloadFavoritesCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildFavoritesCollection();
                            await FavoritesCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadFavorites: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadFavoritesCommand;
            }
        }

        //-- Command para recarregar Comentários
        private ICommand _reloadCommentsCommand;

        public ICommand ReloadCommentsCommand
        {
            get
            {
                if (_reloadCommentsCommand == null)
                {
                    _reloadCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            Loading = true;
                            BuildCommentsCollection();
                            await CommentsCollection.LoadNextPageAsync();
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] ReloadComments: {ex.Message}"); }
                        finally { Loading = false; }
                    });
                }
                return _reloadCommentsCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Submissions
        private ICommand _loadMoreSubmissionsCommand;

        public ICommand LoadMoreSubmissionsCommand
        {
            get
            {
                if (_loadMoreSubmissionsCommand == null)
                {
                    _loadMoreSubmissionsCommand = new RelayCommand(async () =>
                    {
                        if (SubmissionsCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await SubmissionsCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Submissions: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreSubmissionsCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Favorites
        private ICommand _loadMoreFavoritesCommand;

        public ICommand LoadMoreFavoritesCommand
        {
            get
            {
                if (_loadMoreFavoritesCommand == null)
                {
                    _loadMoreFavoritesCommand = new RelayCommand(async () =>
                    {
                        if (FavoritesCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await FavoritesCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Favorites: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreFavoritesCommand;
            }
        }

        //-- Command LoadMore: busca próxima página de Comments
        private ICommand _loadMoreCommentsCommand;

        public ICommand LoadMoreCommentsCommand
        {
            get
            {
                if (_loadMoreCommentsCommand == null)
                {
                    _loadMoreCommentsCommand = new RelayCommand(async () =>
                    {
                        if (CommentsCollection == null || IsLoadingNewPage) return;
                        try
                        {
                            IsLoadingNewPage = true;
                            await CommentsCollection.LoadNextPageAsync();
                        }
                        catch (Exception ex) { Debug.WriteLine($"[AccountVM] LoadMore Comments: {ex.Message}"); }
                        finally { IsLoadingNewPage = false; }
                    });
                }
                return _loadMoreCommentsCommand;
            }
        }

        //-- Command para Follow de Autor
        //-- Command para Follow de Autor
        private ICommand _followAuthor;

        public ICommand FollowAuthor
        {
            get
            {
                if (_followAuthor == null)
                {
                    _followAuthor = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.FollowUser);
                            return;
                        }

                        if (UserAccountInfo.IsFollowing is null)
                        {
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_follow_null_content"
                            });
                            return;
                        }

                        var previousValue = UserAccountInfo.IsFollowing.Value;

                        // optimistic UI
                        UserAccountInfo.IsFollowing = !previousValue;
                        IsFollowing = UserAccountInfo.IsFollowing ?? false;

                        try
                        {
                            Result<bool> result = previousValue
                                ? await _accountService.UnfollowUserAsync(UserAccountInfo.Username)
                                : await _accountService.FollowUserAsync(UserAccountInfo.Username);

                            if (!result.IsSuccess)
                            {
                                // rollback silencioso — API retornou erro mas sem exception
                                UserAccountInfo.IsFollowing = previousValue;
                                IsFollowing = previousValue;

                                _appNotification.AddNotification(new NotificationViewModel
                                {
                                    Message = "notification_follow_error_content"
                                });
                            }
                            else
                            {
                                _appNotification.AddNotification(new NotificationViewModel
                                {
                                    Message = IsFollowing
                                        ? "notification_follow_success_content"
                                        : "notification_unfollow_success_content"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AccountVM] FollowAuthor erro: {ex.Message}");

                            // rollback seguro
                            UserAccountInfo.IsFollowing = previousValue;
                            IsFollowing = previousValue;

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_follow_error_content"
                            });
                        }
                    });
                }
                return _followAuthor;
            }
        }


        //-- Command para abrir a midia
        private ICommand _navigateAccountViewCommand;

        public ICommand NavigateAccountViewCommand
        {
            get
            {
                if (_navigateAccountViewCommand == null)
                {
                    _navigateAccountViewCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(500);
                        _navigator.Navigate("accountView", this);
                    });
                }
                return _navigateAccountViewCommand;
            }
        }

        //-- Command New Post (empty own posts CTA)
        private ICommand _newPostCommand;

        public ICommand NewPostCommand
        {
            get
            {
                if (_newPostCommand == null)
                {
                    _newPostCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            await _dialogService.ShowUploadDialogAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AccountVM] NewPost: {ex.Message}");
                        }
                    });
                }
                return _newPostCommand;
            }
        }

        //-- Command de Back Button
        private ICommand _leaveCurrentPage;

        public ICommand LeaveCurrentPage
        {
            get
            {
                if (_leaveCurrentPage == null)
                {
                    _leaveCurrentPage = new RelayCommand(() =>
                    {
                        _navigator.GoBack();
                    });
                }
                return _leaveCurrentPage;
            }
        }

        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                if (_openSettingsCommand == null)
                {
                    _openSettingsCommand = new RelayCommand(() =>
                    {
                        _navigator.Navigate("settings");
                    });
                }
                return _openSettingsCommand;
            }
        }

        private ICommand _openTermsOfServiceCommand;
        public ICommand OpenTermsOfServiceCommand
        {
            get
            {
                if (_openTermsOfServiceCommand == null)
                {
                    _openTermsOfServiceCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.TermsOfServiceUrl);
                    });
                }
                return _openTermsOfServiceCommand;
            }
        }

        private ICommand _openCcpaCommand;
        public ICommand OpenCcpaCommand
        {
            get
            {
                if (_openCcpaCommand == null)
                {
                    _openCcpaCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.CcpaUrl);
                    });
                }
                return _openCcpaCommand;
            }
        }

        private ICommand _openHelpAndSupportCommand;
        public ICommand OpenHelpAndSupportCommand
        {
            get
            {
                if (_openHelpAndSupportCommand == null)
                {
                    _openHelpAndSupportCommand = new RelayCommand(async () =>
                    {
                        await _uriLauncher.LaunchAsync(AppUrlConstants.HelpAndSupportUrl);
                    });
                }
                return _openHelpAndSupportCommand;
            }
        }

    }
}
