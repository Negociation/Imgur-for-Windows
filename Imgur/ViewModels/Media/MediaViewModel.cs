using Imgur.Services;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Api.Services.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Imgur.Models;
using Imgur.ViewModels.Account;
using Imgur.Factories;
using System.Diagnostics;
using Imgur.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Imgur.ViewModels.Base;

namespace Imgur.ViewModels.Media
{
    public partial class MediaViewModel : CommentableViewModel
    {

        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- Partial loading avaliable
        public bool IsPartialLoading => AlbumPartialLoading.IsPartial(CurrentMedia);

        //-- Remaning Itens if Partial Loading
        public int RemainingItemsCount => CurrentMedia.ImagesCount - 3 ?? 0;

        //-- View States
        public bool IsUpvoted => CurrentMedia.Vote == "up";
        public bool IsDownvoted => CurrentMedia.Vote == "down";
        public bool IsFavorited => CurrentMedia.Favorite;

        public void NotifyFavoriteChanged() => OnPropertyChanged(nameof(IsFavorited));

        //-- Current Media to Show
        private Models.Media _currentMedia;
        public Models.Media CurrentMedia
        {
            get { return _currentMedia; }
            set
            {
                _currentMedia = value;
                OnPropertyChanged("CurrentMedia");
            }
        }

        /// <summary>
        /// Comment opened from Account (or similar) — MediaView shows FocusedCommentDialog while loading.
        /// </summary>
        public CommentViewModel FocusedComment { get; set; }

        //-- Comentários carregados via API
        private List<Comment> _allComments = new List<Comment>();

        //-- Comentários exibidos em tela (Parcial ou Total)
        private ObservableCollection<CommentViewModel> _comments = new ObservableCollection<CommentViewModel>();
        public ObservableCollection<CommentViewModel> Comments
        {
            get { return _comments; }
            set { _comments = value; OnPropertyChanged("Comments"); }
        }

        //-- Status for loadingComments
        private bool _loadingComments;
        public bool LoadingComments
        {
            get { return _loadingComments; }
            set { _loadingComments = value; OnPropertyChanged("LoadingComments"); }
        }

        //-- Status se ha comentários
        private bool _hasComments;
        public bool HasComments
        {
            get { return _hasComments; }
            set { _hasComments = value; OnPropertyChanged("HasComments"); }
        }

        //-- Se há mais comentários adicionais a serem lidos
        private bool _hasMoreComments;
        public bool HasMoreComments
        {
            get { return _hasMoreComments; }
            set { _hasMoreComments = value; OnPropertyChanged("HasMoreComments"); }
        }

        //-- Se esta fazendo carregamento de comentários adicionais
        private bool _loadingMoreComments;
        public bool LoadingMoreComments
        {
            get { return _loadingMoreComments; }
            set { _loadingMoreComments = value; OnPropertyChanged("LoadingMoreComments"); }
        }

        //-- Status for loadingElements
        private bool _loadingElementsStatus;
        public bool LoadingElementsStatus
        {
            get { return _loadingElementsStatus; }
            set
            {
                _loadingElementsStatus = value;
                OnPropertyChanged("LoadingElementsStatus");
            }
        }

        //-- Status for Reloading Current Media Info (Metadata only)
        private bool _reloadingCurrentMediaInfo;
        public bool ReloadingCurrentMediaInfo
        {
            get { return _reloadingCurrentMediaInfo; }
            set
            {
                _reloadingCurrentMediaInfo = value;
                OnPropertyChanged("ReloadingCurrentMediaInfo");
            }
        }

        //-- Status for Appending Current Media Info (Metadata only)
        private bool _isAppendingMediaData;
        public bool IsAppendingMediaData
        {
            get { return _isAppendingMediaData; }
            set
            {
                _isAppendingMediaData = value;
                OnPropertyChanged("IsAppendingMediaData");
            }
        }

        //-- Scroll to Top Button Flag
        private bool _canScrollToTop;
        public bool CanScrollToTop
        {
            get { return _canScrollToTop; }
            set
            {
                _canScrollToTop = value;
                OnPropertyChanged("CanScrollToTop");
            }
        }

        //-- Page Loaded Successfully
        private bool _loadedSuccessfully;
        public bool LoadedSuccessfully
        {
            get { return _loadedSuccessfully; }
            set
            {
                _loadedSuccessfully = value;
                OnPropertyChanged("LoadedSuccessfully");
            }
        }

        //-- Loaded Account Info
        private AccountViewModel _userAccount;
        public AccountViewModel UserAccount
        {
            get { return _userAccount; }
            set
            {
                _userAccount = value;
                OnPropertyChanged("UserAccount");
            }
        }

        private bool _isLoadingAuthor;
        public bool IsLoadingAuthor
        {
            get { return _isLoadingAuthor; }
            set
            {
                _isLoadingAuthor = value;
                OnPropertyChanged(nameof(IsLoadingAuthor));
            }
        }

        //-- Modo de Filtragem de Comentarios
        private bool _filterCommentsByNew;
        public bool FilterCommentsByNew
        {
            get => _filterCommentsByNew;
            set
            {
                if (_filterCommentsByNew == value)
                    return;

                _filterCommentsByNew = value;
                OnPropertyChanged(nameof(FilterCommentsByNew));
            }
        }

        //***************************************************************
        // Services 
        //***************************************************************

        //-- Dispatcher para UI
        private readonly IDispatcher _dispatcher;

        //-- Serviço de Navegação
        private readonly INavigator _navigator;

        //-- Service para busca na Galeria
        private readonly GalleryService _galleryService;

        //-- Service para busca dos dados de conta
        private readonly AccountService _accountService;

        //-- Service de Clipboard (Copy)
        private readonly IClipboardService _clipboard;

        //-- Service de Compartilhamento
        private readonly IShareService _shareService;

        //-- Factory para ViewModel contendo dados dos usuarios
        private readonly IAccountVmFactory _accountVmFactory;

        //-- Factory para ViewModel de Comentários
        private readonly ICommentVmFactory _commentVmFactory;



        //-- Service para execução de Ações de Midia
        private readonly UserMediaActionsService _mediaActionsService;

        //-- Service para Comentários
        private readonly CommentsService _commentService;

        //-- Service para salvar conteúdo em disco
        private readonly IContentStorageService _contentStorageService;

        //-- Service para picker de pasta
        private readonly IFolderDialogService _folderDialogService;

        //-- Service de imagem avulsa
        private readonly ImageService _imageService;

        //-- Service de album básico
        private readonly AlbumService _albumService;

        //-- Histórico local de visualizações
        private readonly IViewHistoryStore _viewHistoryStore;

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************

        //-- Constructor
        public MediaViewModel(
            Models.Media m,
            INavigator navigator,
            IDispatcher dispatcher,
            IClipboardService clipboard,
            IShareService shareService,
            IDialogService dialogService,
            IAppNotificationService appNotification,
            IAccountVmFactory accountVmFactory,
            ICommentVmFactory commentVmFactory,
            IUserContext userContext,
            GalleryService galleryService,
            AccountService accountService,
            UserMediaActionsService mediaActionsService,
            CommentsService commentService,
            IContentStorageService contentStorageService,
            IFolderDialogService folderDialogService,
            ImageService imageService,
            AlbumService albumService,
            IViewHistoryStore viewHistoryStore
            ): base(dialogService, appNotification, userContext)
        {
            _currentMedia = m;
            _navigator = navigator;
            _dispatcher = dispatcher;
            _clipboard = clipboard;
            _accountVmFactory = accountVmFactory;
            _commentVmFactory = commentVmFactory;
            _galleryService = galleryService;
            _accountService = accountService;
            _shareService = shareService;
            _mediaActionsService = mediaActionsService;
            _commentService = commentService;
            _contentStorageService = contentStorageService;
            _folderDialogService = folderDialogService;
            _imageService = imageService;
            _albumService = albumService;
            _viewHistoryStore = viewHistoryStore;
        }

        //-- Initialize
        public void Initialize()
        {
            this.LoadedSuccessfully = true;
            this.LoadingElementsStatus = false;
            _ = RefreshSeenStateAsync();
        }

        private async Task RefreshSeenStateAsync()
        {
            try
            {
                if (CurrentMedia == null || string.IsNullOrWhiteSpace(CurrentMedia.Id) || _viewHistoryStore == null)
                    return;

                var seen = await _viewHistoryStore.HasBeenViewedAsync(CurrentMedia.Id, GetViewHistoryAccountKey());
                _dispatcher.CheckBeginInvokeOnUi(() =>
                {
                    if (CurrentMedia != null)
                        CurrentMedia.IsSeen = seen;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM] RefreshSeenStateAsync: {ex.Message}");
            }
        }

        private async Task RecordFirstViewIfNeededAsync()
        {
            try
            {
                if (CurrentMedia == null || string.IsNullOrWhiteSpace(CurrentMedia.Id) || _viewHistoryStore == null)
                    return;

                await _viewHistoryStore.TryRecordFirstViewAsync(CurrentMedia.Id, GetViewHistoryAccountKey());
                CurrentMedia.IsSeen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM] RecordFirstViewIfNeededAsync: {ex.Message}");
            }
        }

        private string GetViewHistoryAccountKey()
        {
            if (_userContext != null && _userContext.IsAuthenticated)
                return _userContext.CurrentUser?.Username ?? string.Empty;

            return string.Empty;
        }

        //-- Initialize when Page is opened
        public async Task InitializeAsync()
        {
            try
            {
                this.UserAccount = null;
                this.IsLoadingAuthor = true;
                this.LoadedSuccessfully = true;
                this.LoadingElementsStatus = true;

                DumpMediaCollectionDebug("InitializeAsync");

                await Task.Delay(1000);

                // Completa Elements (favorites/listagens sem Images) + Favorite/Vote autenticados
                await RefreshAuthenticatedMediaStateAsync();

                DumpMediaCollectionDebug("AfterEnrich");

                await RecordFirstViewIfNeededAsync();

                // Carrega comentários junto com o resto
                if (LoadCommentsCommand.CanExecute(null))
                    LoadCommentsCommand.Execute(null);

                if (RetrieveUserCommand.CanExecute(null))
                    RetrieveUserCommand.Execute(null);

                this.LoadingElementsStatus = false;
            }
            catch
            {
                this.LoadingElementsStatus = false;
                this.LoadedSuccessfully = false;
                this.IsLoadingAuthor = false;
            }
        }

        /// <summary>
        /// Debug: Id/Link/Cover + cada Element.Link da collection (útil p/ favorites mapping).
        /// </summary>
        private void DumpMediaCollectionDebug(string context)
        {
            try
            {
                var m = CurrentMedia;
                if (m == null)
                {
                    Debug.WriteLine($"[MediaVM:{context}] CurrentMedia=null");
                    return;
                }

                Debug.WriteLine(
                    $"[MediaVM:{context}] Id={m.Id} IsAlbum={m.IsAlbum} ImagesCount={m.ImagesCount} " +
                    $"Elements={(m.Elements?.Count ?? 0)} Link='{m.Link}' CoverLink='{m.CoverLink}' " +
                    $"CoverImage='{m.CoverImage}' CoverUri='{m.CoverUri}' CoverType='{m.CoverType}' CoverVideo='{m.CoverVideo}'");

                if (m.Elements == null || m.Elements.Count == 0)
                {
                    Debug.WriteLine($"[MediaVM:{context}] Elements VAZIO — provável mapping incompleto (favorites album sem Images?)");
                    return;
                }

                for (int i = 0; i < m.Elements.Count; i++)
                {
                    var el = m.Elements[i];
                    Debug.WriteLine(
                        $"[MediaVM:{context}] Element[{i}] Type='{el?.Type}' Link='{el?.Link}' " +
                        $"W={el?.Width} H={el?.Height} Title='{el?.Title}'");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM:{context}] DumpMediaCollectionDebug: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca album/image completo quando Elements veio vazio (ex.: favorites)
        /// e corrige Favorite/Vote relativos ao usuário atual.
        /// </summary>
        private async Task RefreshAuthenticatedMediaStateAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentMedia?.Id))
                    return;

                var hashId = CurrentMedia.Id;
                bool needContent = NeedsElementsHydration();
                bool needAuth = _userContext.IsAuthenticated;

                if (!needContent && !needAuth)
                    return;

                if (!CurrentMedia.IsBasicAlbum)
                {
                    var albumInfo = await _galleryService.GetGalleryAlbumById(hashId);
                    if (!albumInfo.IsSuccess)
                        return;

                    if (needContent)
                        ApplyMediaContent(albumInfo.Data);
                    if (needAuth)
                        ApplyInteractiveMetadata(albumInfo.Data);
                    return;
                }

                if (CurrentMedia.IsAlbum)
                {
                    var album = await _albumService.GetAlbumById(hashId);
                    if (!album.IsSuccess)
                        return;

                    if (needContent)
                        ApplyMediaContent(album.Data);
                    if (needAuth)
                    {
                        CurrentMedia.Favorite = album.Data.Favorite;
                        OnPropertyChanged(nameof(IsFavorited));
                    }
                    return;
                }

                var image = await _imageService.GetImageById(hashId);
                if (!image.IsSuccess)
                    return;

                if (needContent)
                    ApplyMediaContent(image.Data);
                if (needAuth)
                {
                    CurrentMedia.Favorite = image.Data.Favorite;
                    OnPropertyChanged(nameof(IsFavorited));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM] RefreshAuthenticatedMediaState: {ex.Message}");
            }
        }

        private bool NeedsElementsHydration()
        {
            if (CurrentMedia == null)
                return false;

            if (CurrentMedia.Elements == null || CurrentMedia.Elements.Count == 0)
                return true;

            // Álbum com Elements sem Link (mapping incompleto)
            if (CurrentMedia.IsAlbum && CurrentMedia.Elements.Any(e => string.IsNullOrWhiteSpace(e?.Link)))
                return true;

            return false;
        }

        /// <summary>
        /// Copia Elements/capa/link do Media completo para o CurrentMedia aberto na UI.
        /// </summary>
        private void ApplyMediaContent(Models.Media source)
        {
            if (source == null || CurrentMedia == null)
                return;

            if (source.Elements != null && source.Elements.Count > 0)
            {
                if (CurrentMedia.Elements == null)
                    CurrentMedia.Elements = new ObservableCollection<Element>();
                else
                    CurrentMedia.Elements.Clear();

                foreach (var el in source.Elements)
                    CurrentMedia.Elements.Add(el);
            }

            if (source.ImagesCount.HasValue)
                CurrentMedia.ImagesCount = source.ImagesCount;

            if (!string.IsNullOrWhiteSpace(source.Link))
                CurrentMedia.Link = source.Link;

            if (!string.IsNullOrWhiteSpace(source.CoverLink))
                CurrentMedia.CoverLink = source.CoverLink;

            if (!string.IsNullOrWhiteSpace(source.CoverImage))
                CurrentMedia.CoverImage = source.CoverImage;

            if (!string.IsNullOrWhiteSpace(source.CoverUri))
                CurrentMedia.CoverUri = source.CoverUri;

            if (!string.IsNullOrWhiteSpace(source.CoverType))
                CurrentMedia.CoverType = source.CoverType;

            if (!string.IsNullOrWhiteSpace(source.CoverVideo))
                CurrentMedia.CoverVideo = source.CoverVideo;

            if (!string.IsNullOrWhiteSpace(source.Title))
                CurrentMedia.Title = source.Title;

            OnPropertyChanged(nameof(IsPartialLoading));
            OnPropertyChanged(nameof(RemainingItemsCount));
            OnPropertyChanged(nameof(CurrentMedia));
        }

        private void ApplyInteractiveMetadata(Models.Media source)
        {
            if (source == null) return;

            CurrentMedia.Vote = source.Vote;
            CurrentMedia.Favorite = source.Favorite;
            CurrentMedia.Likes = source.Likes;
            CurrentMedia.Ups = source.Ups;
            CurrentMedia.Downs = source.Downs;
            CurrentMedia.CommentCount = source.CommentCount;
            CurrentMedia.Votes = source.Votes;
            OnPropertyChanged(nameof(IsFavorited));
            OnPropertyChanged(nameof(IsUpvoted));
            OnPropertyChanged(nameof(IsDownvoted));
        }



        //***************************************************************
        // View Commands 
        //***************************************************************
    }
}
