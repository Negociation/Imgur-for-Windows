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

namespace Imgur.ViewModels.Media
{


    public class MediaViewModel : Observable
    {

        //***************************************************************
        // View Parameters 
        //***************************************************************

        //-- Partial loading avaliable
        public bool IsPartialLoading => CurrentMedia.IsAlbum &&( CurrentMedia.ImagesCount != CurrentMedia.Elements.Count);

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

        //-- Usuario Autenticado
        public bool IsAuthenticated => _userContext.IsAuthenticated;

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


        //-- Service para Envio de Notificações
        private readonly IAppNotificationService _appNotification;

        //-- Factory para ViewModel contendo dados dos usuarios
        private readonly IAccountVmFactory _accountVmFactory;

        //-- Service para Dialogos 
        private readonly IDialogService _dialogService;

        //-- UserContext
        private readonly IUserContext _userContext;

        //Ações de Midia
        private readonly UserMediaActionsService _mediaActionsService;

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
            IUserContext userContext,
            GalleryService galleryService,
            AccountService accountService,
            UserMediaActionsService mediaActionsService
            )
        {
            _currentMedia = m;
            _navigator = navigator;
            _dispatcher = dispatcher;
            _appNotification = appNotification;
            _clipboard = clipboard;
            _accountVmFactory = accountVmFactory;
            _galleryService = galleryService;
            _accountService = accountService;
            _shareService = shareService;
            _dialogService = dialogService;
            _userContext = userContext;
            _mediaActionsService = mediaActionsService;
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
                if (RetrieveUserCommand.CanExecute(null)) { RetrieveUserCommand.Execute(null); }
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
                                    {
                                        this.UserAccount = this._accountVmFactory.GetAccountViewModel(account.Data);
                                    }
                                    else
                                    {
                                        throw new Exception("User Account couldn't be reached");
                                    }
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
                                {
                                    throw new Exception("Erro durante a busca dos metadados de album: " + AlbumInfo.Error);
                                }

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

        //-- Command para carregar demais itens
        private ICommand _loadRemaningItensCommand;

        public ICommand LoadRemaningItensCommand
        {
            get
            {
                if (_loadRemaningItensCommand == null)
                {
                    _loadRemaningItensCommand = new RelayCommand(async () =>
                    {
                        //Start Loading Indicator
                        IsAppendingMediaData = true;

                        //Visual Delay
                        await Task.Delay(1000);

                        //Get Current Media Id
                        var hashId = this.CurrentMedia.Id;

                        //Get Gallery Album Fully
                        var albumInfo = await _galleryService.GetGalleryAlbumById(hashId);

                        //If not Success Show error
                        if (!albumInfo.IsSuccess)
                        {
                            IsAppendingMediaData = false;
                            throw new Exception("Erro durante o carregamento dos elementos extras do album: " + albumInfo.Error);
                        }

                        //If Scucess Stop Loading Indicator
                        IsAppendingMediaData = false;

                        if (albumInfo?.Data.Elements == null || albumInfo.Data.Elements.Count <= 3)
                            return;

                        // Começa a partir do índice 3
                        for (int i = 3; i < albumInfo.Data.Elements.Count; i++)
                        {
                            CurrentMedia.Elements.Add(albumInfo.Data.Elements[i]);
                        }

                       
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
                    _copy = new RelayCommand(async () => {
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
                        ;
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

                        // Atualiza localmente imediato
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

                        // Se clicar no mesmo voto → remove (toggle)
                        var previous = CurrentMedia.Vote;
                        CurrentMedia.Vote = CurrentMedia.Vote == vote ? null : vote;

                        OnPropertyChanged(nameof(IsUpvoted));
                        OnPropertyChanged(nameof(IsDownvoted));

                        // Atualiza contadores localmente
                        if (vote == "up")
                        {
                            if (previous == "up")
                                CurrentMedia.Ups--;          
                            else if (previous == "down")
                            {
                                CurrentMedia.Downs--;        
                                CurrentMedia.Ups++;          
                            }
                            else
                                CurrentMedia.Ups++;
                        }
                        else if (vote == "down")
                        {
                            if (previous == "down")
                                CurrentMedia.Downs--;

                            else if (previous == "up")
                            {
                                CurrentMedia.Ups--;
                                CurrentMedia.Downs++;
                            }
                            else
                                CurrentMedia.Downs++;
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


    }
}
