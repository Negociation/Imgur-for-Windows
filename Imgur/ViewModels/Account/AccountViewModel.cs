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
    public partial class AccountViewModel : Observable
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
    }
}
