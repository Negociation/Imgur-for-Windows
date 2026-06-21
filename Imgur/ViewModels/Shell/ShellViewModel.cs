using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Helpers;
using Imgur.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Imgur.ViewModels.Shell
{
    public class ShellViewModel : Observable
    {

        public int CurrentPageIndex { get; set; }
        public bool IsAuthenticated => _userContext.IsAuthenticated;
        public bool IsNotAuthenticated => !_userContext.IsAuthenticated;

        private bool _isFullScreenMode;

        public bool IsFullScreenMode
        {
            get => _isFullScreenMode;
            private set
            {
                if (_isFullScreenMode != value)
                {
                    _isFullScreenMode = value;
                    OnPropertyChanged(nameof(IsFullScreenMode));
                }
            }
        }

        public bool HasCustomApiCredentials =>
        !string.IsNullOrWhiteSpace(_localSettings.Get<string>(LocalSettingsConstants.CustomClientId)) &&
        !string.IsNullOrWhiteSpace(_localSettings.Get<string>(LocalSettingsConstants.CustomClientSecret));

        public User CurrentUser => _userContext.CurrentUser;

        private readonly INavigator _navigator;
        private readonly IAppNotificationService _notification;
        private readonly IUserContext _userContext;
        private readonly IDialogService _dialogService;
        private readonly ILocalSettings _localSettings;
        private readonly IAccountVmFactory _accountVmFactory;


        private readonly IExplorerSearchVmFactory _explorerSearchVmFactory;

        public ObservableCollection<NotificationViewModel> NotificationStack { get; } = new ObservableCollection<NotificationViewModel>();
        public ShellViewModel(
            INavigator Navigator,
            IAppNotificationService notification,
            IDialogService dialogService,
            IExplorerSearchVmFactory explorerSearchVmFactory,
            IUserContext userContext,
            ILocalSettings localSettings,
            IAccountVmFactory accountVmFactory
            )
        {
            _navigator = Navigator;
            _notification = notification;
            _dialogService = dialogService;
            _userContext = userContext;
            _explorerSearchVmFactory = explorerSearchVmFactory;
            _localSettings = localSettings;
            _accountVmFactory = accountVmFactory;

            _userContext.OnAuthenticationChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(IsNotAuthenticated));
                OnPropertyChanged(nameof(CurrentUser));
            };

        }

        private ICommand _navigateToCommand;

        public ICommand NavigateToCommand
        {
            get
            {
                if (_navigateToCommand == null)
                {
                    _navigateToCommand = new RelayCommand<string>((route) => {
                        _navigator.Navigate(route);
                    });
                }
                return _navigateToCommand;
            }
        }


        private ICommand _invokeUploadCommand;

        public ICommand InvokeUploadCommand
        {
            get
            {
                if (_invokeUploadCommand == null)
                {
                    _invokeUploadCommand = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            var loginResult = await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Upload);
                            if (loginResult == true)
                            {
                                if (!this.HasCustomApiCredentials)
                                {
                                    await _dialogService.ShowUploadInterceptorDialog();
                                    return;
                                }
                                await _dialogService.ShowUploadDialogAsync();
                            }
                            return;
                        }
                        if (!this.HasCustomApiCredentials)
                        {
                            await _dialogService.ShowUploadInterceptorDialog();
                            return;
                        }
                        await _dialogService.ShowUploadDialogAsync();
                    });
                }
                return _invokeUploadCommand;
            }
        }


        private ICommand _invokeSearchCommand;

        public ICommand InvokeSearchCommand
        {
            get
            {
                if (_invokeSearchCommand == null)
                {
                    _invokeSearchCommand = new RelayCommand<string>((query) =>
                    {
                        //Setar Index para Explorer Page
                        CurrentPageIndex = 1;
                        OnPropertyChanged(nameof(CurrentPageIndex));

                        //Fabrica de ViewModel com parametro de busca
                        var vm = _explorerSearchVmFactory.getSearchViewModel(query);

                        //Navegar com ViewModel com valor de Search
                        _navigator.Navigate("explorerSearch", vm);
                    });
                }
                return _invokeSearchCommand;
            }
        }

        private ICommand _openOwnAccountCommand;

        public ICommand OpenOwnAccountCommand
        {
            get
            {
                if (_openOwnAccountCommand == null)
                {
                    _openOwnAccountCommand = new RelayCommand<string>((query) =>
                    {
                        CurrentPageIndex = -1;
                        OnPropertyChanged(nameof(CurrentPageIndex));
                        if (IsAuthenticated)
                        {
                            var vm = this._accountVmFactory.GetAccountViewModel(_userContext.CurrentUser);
                            _navigator.Navigate("accountView", vm);
                        }

                    

                    });
                }
                return _openOwnAccountCommand;
            }
        }



        private ICommand _invokeLoginCommand;

        public ICommand InvokeLoginCommand
        {
            get
            {
                if (_invokeLoginCommand == null)
                {
                    _invokeLoginCommand = new RelayCommand(async () =>
                    {
                        if (!IsAuthenticated)
                        {
                            await _userContext.LoginAsync();
                        }
                    });
                }
                return _invokeLoginCommand;
            }
        }


        private ICommand _invokeLogoutCommand;

        public ICommand InvokeLogoutCommand
        {
            get
            {
                if (_invokeLogoutCommand == null)
                {
                    _invokeLogoutCommand = new RelayCommand(async () =>
                    {
                        if (IsAuthenticated)
                        {
                            await _userContext.LogoutAsync();
                        }
                    });
                }
                return _invokeLogoutCommand;
            }
        }

        public void Initialize()
        {
            //Get Events
            this._notification.NotificationAdded += OnNotificationRecieved;
            this._navigator.FullScreenModeChanged += OnViewPageFullscreenModeChanged;
            this._navigator.NavigateInvoked += NavigateInvoked;
            this.CurrentPageIndex = 0;
        }

        private void NavigateInvoked(object sender, string e)
        {
            int index;
            switch (e)
            {
                case "explorer":
                    index = 0;
                    break;
                case "explorerSearch":
                    index = 1;
                    break;
                case "settings":
                    index = 4;
                    break;
                default:
                    index = -1;
                    break;
            }
            if (index != CurrentPageIndex)
            {
                CurrentPageIndex = index;
                OnPropertyChanged(nameof(CurrentPageIndex));
            }
        }

        private void OnNotificationRecieved(object sender, NotificationViewModel e)
        {
            this.NotificationStack.Add(e);
            OnPropertyChanged(nameof(NotificationStack));
        }

        private void OnViewPageFullscreenModeChanged(object sender, bool e)
        {
            IsFullScreenMode = e;
        }
    }
}
