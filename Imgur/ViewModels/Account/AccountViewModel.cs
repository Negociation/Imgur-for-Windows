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

        public bool HasFavorites => (FavoritesCollection?.Count ?? 0) > 0;
        public bool HasSubmissions => (SubmissionsCollection?.Count ?? 0) > 0;
        public bool HasComments => (CommentsCollection?.Count ?? 0) > 0;

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
            private set { _isSubmissionsEmpty = value; OnPropertyChanged(nameof(IsSubmissionsEmpty)); }
        }

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

        public bool IsUserOwnPage => _userContext.CurrentUser.Username == _userAccountInfo.Username;

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
            IAppNotificationService appNotification
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



            BuildCollections();
        }

        //-- View Model Initilization
        public async Task InitializeAsync()
        {
            if (LoadedSuccessfully) return;

            try{
               OnPropertyChanged("IsUserOwnPage");
               Loading = true;
               await Task.WhenAll(
                    FavoritesCollection.LoadNextPageAsync(),
                    SubmissionsCollection.LoadNextPageAsync(),
                    CommentsCollection.LoadNextPageAsync()
                );

                if (!IsUserOwnPage && UserAccountInfo.IsFollowing == null)
                {
                    var result = await _accountService.GetFollowStatusAsync(UserAccountInfo.Username);
                    if (!result.IsSuccess)
                        UserAccountInfo.IsFollowing = result.Data;
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

        private void BuildSubmissionsCollection()
        {
            if (SubmissionsCollection != null)
                SubmissionsCollection.StateChanged -= OnSubmissionsStateChanged;

            CanLoadMoreSubmissions = false;
            IsSubmissionsEmpty = false;
            var username = UserAccountInfo?.Username;

            SubmissionsCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountSubmissionsAsync(username, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    return result.Data.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: 60,
                batchSize: 10,
                autoTriggerNextPage: false);

            SubmissionsCollection.StateChanged += OnSubmissionsStateChanged;
            OnPropertyChanged(nameof(SubmissionsCollection));
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



    }
}
