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


        //-- Usuario Atual Autenticado
        public bool IsAuthenticated => _userContext.IsAuthenticated;

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

        public AccountViewModel(
            UserAccount account,
            IUserContext userContext,
            IDialogService dialogService,
            INavigator navigator,
            ILocalSettings localSettings,
            IIncrementalCollectionFactory collectionFactory,
            Imgur.Services.AccountService accountService,
            IMediaVmFactory mediaVmFactory,
            ICommentVmFactory commentVmFactory
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


            BuildCollections();
        }

        //-- View Model Initilization
        public async Task InitializeAsync()
        {
            if (LoadedSuccessfully) return;

            try{
               Loading = true;
               await Task.WhenAll(
                    FavoritesCollection.LoadNextPageAsync(),
                    SubmissionsCollection.LoadNextPageAsync(),
                    CommentsCollection.LoadNextPageAsync()
                );
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
            var username = UserAccountInfo?.Username;

            // Favoritos
            FavoritesCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountFavoritesAsync(username, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    return result.Data.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: 30,
                batchSize: 10);

            FavoritesCollection.StateChanged += (s, e) => OnPropertyChanged(nameof(HasFavorites));

            // Submissions
            SubmissionsCollection = _collectionFactory.Create<MediaViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountSubmissionsAsync(username, page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<MediaViewModel>();

                    return result.Data.Select(_mediaVmFactory.GetMediaViewModel).ToList();
                },
                pageSize: 30,
                batchSize: 10);

            SubmissionsCollection.StateChanged += (s, e) => OnPropertyChanged(nameof(HasSubmissions));

            // Comments
            CommentsCollection = _collectionFactory.Create<CommentViewModel>(
                async (page, ct) =>
                {
                    var result = await _accountService.GetAccountCommentsAsync(username, "newest", page);
                    if (!result.IsSuccess || result.Data == null)
                        return new List<CommentViewModel>();
                    /*
                    return result.Data
                        .Select(c => _commentVmFactory.GetCommentViewModel(c, string.Empty))
                        .ToList();
                        */

                    return new List<CommentViewModel>();

                },
                pageSize: 30,
                batchSize: 10);

            CommentsCollection.StateChanged += (s, e) => OnPropertyChanged(nameof(HasComments));

            OnPropertyChanged(nameof(FavoritesCollection));
            OnPropertyChanged(nameof(SubmissionsCollection));
            OnPropertyChanged(nameof(CommentsCollection));
        }

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
                        };
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
