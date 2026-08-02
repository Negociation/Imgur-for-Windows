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
    public class MediaViewModel : CommentableViewModel
    {

        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- Partial loading avaliable
        public bool IsPartialLoading => CurrentMedia.IsAlbum && (CurrentMedia.ImagesCount != CurrentMedia.Elements.Count);

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

        //-- Command para Busca dos dados do Autor da Postagem
        private ICommand _retrieveUserCommand;
        public ICommand RetrieveUserCommand
        {
            get
            {
                if (_retrieveUserCommand == null)
                {
                    _retrieveUserCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            IsLoadingAuthor = true;
                            UserAccount = null;

                            if (!this.CurrentMedia.IsBasicAlbum)
                            {
                                if (!string.IsNullOrEmpty(this.CurrentMedia.AccountId))
                                {
                                    var account = await this._accountService.GetAccountById(this.CurrentMedia.AccountId);
                                    if (account.IsSuccess)
                                        this.UserAccount = this._accountVmFactory.GetAccountViewModel(account.Data);
                                    else
                                        throw new Exception("User Account couldn't be reached");
                                }
                                else
                                {
                                    var account = Imgur.Models.UserAccount.CreateAnonymous();
                                    this.UserAccount = this._accountVmFactory.GetAccountViewModel(account);
                                }
                            }
                            else
                            {
                                var account = Imgur.Models.UserAccount.CreateAnonymous();
                                this.UserAccount = this._accountVmFactory.GetAccountViewModel(account);
                            }

                            this.LoadedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            this.LoadedSuccessfully = false;
                            this.UserAccount = null;
                        }
                        finally
                        {
                            IsLoadingAuthor = false;
                        }
                    });
                }
                return _retrieveUserCommand;
            }
        }

        //-- Comando para refresh somente do Metadata da Postagem
        private ICommand _reloadCurrentMediaInfo;
        public ICommand ReloadCurrentMediaInfo
        {
            get
            {
                if (_reloadCurrentMediaInfo == null)
                {
                    _reloadCurrentMediaInfo = new RelayCommand(async () =>
                    {
                        try
                        {
                            ReloadingCurrentMediaInfo = true;

                            await Task.Delay(1000);

                            var hashId = this.CurrentMedia.Id;
                            if (!CurrentMedia.IsBasicAlbum)
                            {
                                var AlbumInfo = await _galleryService.GetGalleryAlbumById(hashId);

                                if (!AlbumInfo.IsSuccess)
                                    throw new Exception("Erro durante a busca dos metadados de album: " + AlbumInfo.Error);

                                if (NeedsElementsHydration())
                                    ApplyMediaContent(AlbumInfo.Data);
                                ApplyInteractiveMetadata(AlbumInfo.Data);
                            }
                            else
                            {
                                await RefreshAuthenticatedMediaStateAsync();
                            }
                            LoadedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            LoadedSuccessfully = false;
                        }
                        finally
                        {
                            ReloadingCurrentMediaInfo = false;
                        }
                    });
                }
                return _reloadCurrentMediaInfo;
            }
        }

        //-- Command para carregar demais itens de mídia
        private ICommand _loadRemaningItensCommand;
        public ICommand LoadRemaningItensCommand
        {
            get
            {
                if (_loadRemaningItensCommand == null)
                {
                    _loadRemaningItensCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(1000);
                        var ok = await EnsureRemainingAlbumElementsAsync();
                        if (!ok)
                            throw new Exception("Erro durante o carregamento dos elementos extras do album.");
                    });
                }
                return _loadRemaningItensCommand;
            }
        }

        /// <summary>
        /// Completa <see cref="Media.Elements"/> quando o álbum veio só com os primeiros itens.
        /// </summary>
        private async Task<bool> EnsureRemainingAlbumElementsAsync()
        {
            if (CurrentMedia == null || !CurrentMedia.IsAlbum)
                return true;

            if (!IsPartialLoading)
                return true;

            if (IsAppendingMediaData)
            {
                // Já há um load em curso (botão "carregar mais") — espera terminar.
                while (IsAppendingMediaData)
                    await Task.Delay(100);

                return !IsPartialLoading || (CurrentMedia.Elements?.Count ?? 0) > 0;
            }

            IsAppendingMediaData = true;
            try
            {
                var albumInfo = await _galleryService.GetGalleryAlbumById(CurrentMedia.Id);
                if (!albumInfo.IsSuccess || albumInfo.Data?.Elements == null)
                    return false;

                if (CurrentMedia.Elements == null)
                    CurrentMedia.Elements = new ObservableCollection<Element>();

                var already = CurrentMedia.Elements.Count;
                for (int i = already; i < albumInfo.Data.Elements.Count; i++)
                    CurrentMedia.Elements.Add(albumInfo.Data.Elements[i]);

                if (albumInfo.Data.ImagesCount.HasValue)
                    CurrentMedia.ImagesCount = albumInfo.Data.ImagesCount;

                OnPropertyChanged(nameof(IsPartialLoading));
                OnPropertyChanged(nameof(RemainingItemsCount));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaVM] EnsureRemainingAlbumElementsAsync: {ex.Message}");
                return false;
            }
            finally
            {
                IsAppendingMediaData = false;
            }
        }

        //-- Command para abrir a midia
        private ICommand _navigateMediaCommand;
        public ICommand NavigateMediaCommand
        {
            get
            {
                if (_navigateMediaCommand == null)
                {
                    _navigateMediaCommand = new RelayCommand(async () =>
                    {
                        await Task.Delay(100);
                        _navigator.Navigate("media", this);
                    });
                }
                return _navigateMediaCommand;
            }
        }

        //-- Command para Copiar para o Clipboard
        private ICommand _copy;
        public ICommand Copy
        {
            get
            {
                if (_copy == null)
                {
                    _copy = new RelayCommand(async () =>
                    {
                        if (await this._clipboard.GetTextAsync() != this.CurrentMedia.Link)
                        {
                            this._clipboard.SetText(this.CurrentMedia.Link);
                            NotificationViewModel notification = new NotificationViewModel();
                            notification.Message = "notification_clipboard_content";
                            this._appNotification.AddNotification(notification);
                        }
                    });
                }
                return _copy;
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

        //-- Command para Invocar Embed Dialog
        private ICommand _invokeEmbedDialog;
        public ICommand InvokeEmbedDialog
        {
            get
            {
                if (_invokeEmbedDialog == null)
                {
                    _invokeEmbedDialog = new RelayCommand(async () =>
                    {
                        await _dialogService.ShowEmbedDialogAsync(CurrentMedia);
                    });
                }
                return _invokeEmbedDialog;
            }
        }

        //-- Command para Invocar Share Dialog
        private ICommand _invokeShareDialog;
        public ICommand InvokeShareDialog
        {
            get
            {
                if (_invokeShareDialog == null)
                {
                    _invokeShareDialog = new RelayCommand(() =>
                    {
                        this._shareService.ShareMediaPost(this.CurrentMedia.Title, this.CurrentMedia.Link);
                    });
                }
                return _invokeShareDialog;
            }
        }

        //-- Command para Download de Elemento de Mídia (recebe URL como parâmetro)
        private ICommand _downloadElementCommand;
        public ICommand DownloadElementCommand
        {
            get
            {
                if (_downloadElementCommand == null)
                {
                    _downloadElementCommand = new RelayCommand<string>(async (url) =>
                    {
                        if (string.IsNullOrEmpty(url)) return;

                        try
                        {
                            var folderToken = await _folderDialogService.PickFolderAsync();
                            if (folderToken == null) return; // utilizador cancelou o picker

                            // Infere o nome do ficheiro a partir da URL
                            var uri = new Uri(url);
                            var fileName = Path.GetFileName(uri.LocalPath);
                            if (string.IsNullOrWhiteSpace(fileName))
                                fileName = $"download_{DateTime.Now:yyyyMMdd_HHmmss}";

                            await _contentStorageService.SaveAsync(uri, folderToken, fileName);

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_download_success_content"
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] DownloadElementCommand erro: {ex.Message}");
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_download_error_content"
                            });
                        }
                    });
                }
                return _downloadElementCommand;
            }
        }

        //-- Command para abrir elemento (imagem/vídeo) em fullscreen
        private ICommand _openElementFullScreenCommand;
        public ICommand OpenElementFullScreenCommand
        {
            get
            {
                if (_openElementFullScreenCommand == null)
                    _openElementFullScreenCommand = new RelayCommand<Element>(OpenElementFullScreen);
                return _openElementFullScreenCommand;
            }
        }

        //-- Command para Favoritar Midia
        private ICommand _favourite;
        public ICommand Favourite
        {
            get
            {
                if (_favourite == null)
                {
                    _favourite = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Like);
                            return;
                        }

                        var previous = CurrentMedia.Favorite;
                        CurrentMedia.Favorite = !CurrentMedia.Favorite;
                        OnPropertyChanged(nameof(IsFavorited));

                        var result = await _mediaActionsService.FavoriteAsync(CurrentMedia.Id, CurrentMedia.IsAlbum);
                        if (!result.IsSuccess)
                        {
                            CurrentMedia.Favorite = previous;
                            OnPropertyChanged(nameof(IsFavorited));

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_favourite_error_content"
                            });
                        }
                        else if (CurrentMedia.Favorite)
                        {
                            // Notifica apenas ao favoritar — ao desfavoritar não exibe nada
                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_favourite_success_content"
                            });
                        }
                    });
                }
                return _favourite;
            }
        }

        //-- Command para Vote ( Upvote ("up") | Downvote ("down") )
        private ICommand _vote;
        public ICommand Vote
        {
            get
            {
                if (_vote == null)
                {
                    _vote = new RelayCommand<string>(async (vote) =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Vote);
                            return;
                        }

                        var previous = CurrentMedia.Vote;
                        var upvoteOnly = false;
                        if (!string.IsNullOrEmpty(vote) && vote.EndsWith(":only", StringComparison.OrdinalIgnoreCase))
                        {
                            upvoteOnly = true;
                            vote = vote.Substring(0, vote.Length - ":only".Length);
                        }

                        // Double-tap / upvoteOnly: se já está up, não inverte (não remove o voto)
                        if (upvoteOnly && vote == "up" && previous == "up")
                            return;

                        CurrentMedia.Vote = CurrentMedia.Vote == vote ? null : vote;

                        OnPropertyChanged(nameof(IsUpvoted));
                        OnPropertyChanged(nameof(IsDownvoted));

                        if (vote == "up")
                        {
                            if (previous == "up") CurrentMedia.Ups--;
                            else if (previous == "down") { CurrentMedia.Downs--; CurrentMedia.Ups++; }
                            else CurrentMedia.Ups++;
                        }
                        else if (vote == "down")
                        {
                            if (previous == "down") CurrentMedia.Downs--;
                            else if (previous == "up") { CurrentMedia.Ups--; CurrentMedia.Downs++; }
                            else CurrentMedia.Downs++;
                        }

                        OnPropertyChanged(nameof(CurrentMedia));

                        var voteValue = CurrentMedia.Vote ?? "veto";
                        var result = await _mediaActionsService.VoteAsync(CurrentMedia.Id, voteValue);
                        if (!result.IsSuccess)
                        {
                            CurrentMedia.Vote = previous;
                            OnPropertyChanged(nameof(IsUpvoted));
                            OnPropertyChanged(nameof(IsDownvoted));

                            _appNotification.AddNotification(new NotificationViewModel
                            {
                                Message = "notification_vote_error_content"
                            });
                        }
                        else
                        {
                            CurrentMedia.Votes = CurrentMedia.Ups - CurrentMedia.Downs;
                        }
                    });
                }
                return _vote;
            }
        }

        //-- Command para Inicialização
        private ICommand _initializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (_initializeCommand == null)
                {
                    _initializeCommand = new RelayCommand(async () =>
                    {
                        await this.InitializeAsync();
                    });
                }
                return _initializeCommand;
            }
        }

        //-- Command para buscar e carregar comentários (parcial — primeiros 5)
        private ICommand _loadCommentsCommand;
        public ICommand LoadCommentsCommand
        {
            get
            {
                if (_loadCommentsCommand == null)
                {
                    _loadCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            LoadingComments = true;

                            var filter = _filterCommentsByNew ? "new" : "best";
                            var result = await _commentService.GetCommentsAsync(CurrentMedia.Id, filter);

                            if (result.IsSuccess)
                            {
                                _allComments = result.Data;

                                Comments.Clear();

                                foreach (var comment in _allComments.Take(5))
                                    Comments.Add(CreateCommentViewModel(comment));

                                HasComments = Comments.Count > 0;
                                HasMoreComments = _allComments.Count > 5;

                                Debug.WriteLine($"[MediaVM] {Comments.Count} comentários carregados.");
                            }
                            else
                            {
                                Debug.WriteLine($"[MediaVM] Erro ao carregar comentários: {result.ErrorType}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] Exceção ao carregar comentários: {ex.Message}");
                        }
                        finally
                        {
                            LoadingComments = false;
                        }
                    });
                }
                return _loadCommentsCommand;
            }
        }

        //-- Command para carregar em tela os comentários em Memória restantes
        private ICommand _loadAllCommentsCommand;
        public ICommand LoadAllCommentsCommand
        {
            get
            {
                if (_loadAllCommentsCommand == null)
                {
                    _loadAllCommentsCommand = new RelayCommand(async () =>
                    {
                        try
                        {
                            LoadingMoreComments = true;

                            await Task.Delay(300);

                            foreach (var comment in _allComments.Skip(5))
                                Comments.Add(CreateCommentViewModel(comment));

                            HasMoreComments = false;

                            Debug.WriteLine($"[MediaVM] Todos os {_allComments.Count} comentários carregados.");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MediaVM] Exceção LoadAll: {ex.Message}");
                        }
                        finally
                        {
                            LoadingMoreComments = false;
                        }
                    });
                }
                return _loadAllCommentsCommand;
            }
        }

        //-- Command para reply em comentário
        private ICommand _replyCommentCommand;
        public ICommand ReplyCommentCommand
        {
            get
            {
                if (_replyCommentCommand == null)
                {
                    _replyCommentCommand = new RelayCommand<Comment>(async (comment) =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Comment);
                            return;
                        }
                        // TODO: abrir dialog de reply
                    });
                }
                return _replyCommentCommand;
            }
        }

        //-- Command para atualizar o filtro de comentários
        private ICommand _toggleCommentFilter;
        public ICommand ToggleCommentFilter
        {
            get
            {
                if (_toggleCommentFilter == null)
                {
                    _toggleCommentFilter = new RelayCommand(() =>
                    {
                        FilterCommentsByNew = !FilterCommentsByNew;

                        LoadCommentsCommand.Execute(null);
                    });
                }

                return _toggleCommentFilter;
            }
        }

        protected override async Task OnStartCommentAsync()
        {
            await _dialogService.ShowCommentDialog(this);
        }

        protected override async Task<bool> OnSubmitCommentAsync(string text)
        {
            var result = await _commentService.PostCommentAsync(CurrentMedia.Id, CommentText);

            await Task.Delay(1000);

            if (result.IsSuccess)
            {
                // Recarrega os comentários
                if (LoadCommentsCommand.CanExecute(null))
                    LoadCommentsCommand.Execute(null);
            }

            return result.IsSuccess;
        }

        private CommentViewModel CreateCommentViewModel(Comment comment)
        {
            var vm = _commentVmFactory.GetMediaCommentViewModel(CurrentMedia.Id, comment);
            vm.OpenAlbumPosition = OpenAlbumElementAtIndex;
            return vm;
        }

        /// <summary>
        /// Abre o elemento do álbum no índice 0-based (a partir de #N nos comentários).
        /// Se o álbum estiver parcialmente carregado e o índice ainda não existir, carrega o resto e tenta de novo.
        /// Ignora se o índice estiver fora de <see cref="Media.ImagesCount"/>.
        /// </summary>
        private async void OpenAlbumElementAtIndex(int zeroBasedIndex)
        {
            if (CurrentMedia?.Elements == null || zeroBasedIndex < 0)
                return;

            if (zeroBasedIndex >= CurrentMedia.Elements.Count)
            {
                var totalKnown = CurrentMedia.ImagesCount ?? CurrentMedia.Elements.Count;
                if (!CurrentMedia.IsAlbum || zeroBasedIndex >= totalKnown)
                    return;

                var loaded = await EnsureRemainingAlbumElementsAsync();
                if (!loaded || zeroBasedIndex >= CurrentMedia.Elements.Count)
                    return;
            }

            OpenElementFullScreen(CurrentMedia.Elements[zeroBasedIndex]);
        }

        private void OpenElementFullScreen(Element element)
        {
            if (element == null || string.IsNullOrEmpty(element.Link))
                return;

            _navigator.NavigateElementToFullScreen(new FullScreenElementArgs
            {
                Element = element,
                Post = CurrentMedia,
                AuthorUsername = UserAccount?.UserAccountInfo?.Username,
                FavoriteChanged = NotifyFavoriteChanged
            });
        }
    }
}