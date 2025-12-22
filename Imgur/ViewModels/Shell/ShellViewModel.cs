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


        public User CurrentUser => _userContext.CurrentUser;

        private readonly INavigator _navigator;
        private readonly IAppNotificationService _notification;
        private readonly IUserContext _userContext;
        private readonly IDialogService _dialogService;

        private readonly IExplorerSearchVmFactory _explorerSearchVmFactory;

        public ObservableCollection<Notification> NotificationStack { get; } = new ObservableCollection<Notification>();
        public ShellViewModel(
            INavigator Navigator,
            IAppNotificationService notification,
            IDialogService dialogService,
            IExplorerSearchVmFactory explorerSearchVmFactory,
            IUserContext userContext
            )
        {
            _navigator = Navigator;
            _notification = notification;
            _dialogService = dialogService;
            _userContext = userContext;
            _explorerSearchVmFactory = explorerSearchVmFactory;
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
                            await _dialogService.ShowLoginInterceptorDialog(LoginInterceptorEnum.Upload);
                            return;
                        }
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

        public void Initialize()
        {
            //Get Events
            this._notification.NotificationAdded += OnNotificationRecieved;
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

        private void OnNotificationRecieved(object sender, Notification e)
        {
            this.NotificationStack.Add(e);
            OnPropertyChanged(nameof(NotificationStack));
        }
        
    }
}
