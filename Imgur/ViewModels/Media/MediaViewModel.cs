using Imgur.Services;
using Imgur.Contracts;
using Imgur.Helpers;
using System;
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
            CommentsService commentService
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
        }

        //-- Initialize
        public void Initialize()
        {
            this.LoadedSuccessfully = true;
            this.LoadingElementsStatus = false;
        }

        //-- Initialize when Page is opened
        public async Task InitializeAsync()
        {
            try
            {
                this.LoadedSuccessfully = true;
                this.LoadingElementsStatus = true;

                await Task.Delay(1000);

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
            }
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
                            this.LoadedSuccessfully = true;
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
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            this.LoadedSuccessfully = false;
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

                                CurrentMedia.Vote = AlbumInfo.Data.Vote;
                                CurrentMedia.Favorite = AlbumInfo.Data.Favorite;
                                CurrentMedia.Likes = AlbumInfo.Data.Likes;
                                CurrentMedia.Ups = AlbumInfo.Data.Ups;
                                CurrentMedia.Downs = AlbumInfo.Data.Downs;
                                CurrentMedia.CommentCount = AlbumInfo.Data.CommentCount;
                                CurrentMedia.Votes = AlbumInfo.Data.Votes;
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
                        IsAppendingMediaData = true;

                        await Task.Delay(1000);

                        var hashId = this.CurrentMedia.Id;
                        var albumInfo = await _galleryService.GetGalleryAlbumById(hashId);

                        if (!albumInfo.IsSuccess)
                        {
                            IsAppendingMediaData = false;
                            throw new Exception("Erro durante o carregamento dos elementos extras do album: " + albumInfo.Error);
                        }

                        IsAppendingMediaData = false;

                        if (albumInfo?.Data.Elements == null || albumInfo.Data.Elements.Count <= 3)
                            return;

                        for (int i = 3; i < albumInfo.Data.Elements.Count; i++)
                            CurrentMedia.Elements.Add(albumInfo.Data.Elements[i]);

                        OnPropertyChanged(nameof(IsPartialLoading));
                    });
                }
                return _loadRemaningItensCommand;
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

                                    Comments.Add(_commentVmFactory.GetCommentViewModel(CurrentMedia.Id ,comment));

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
                                Comments.Add(_commentVmFactory.GetCommentViewModel(CurrentMedia.Id, comment));

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
    }
}