using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Helpers;
using System;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public class LoginInterceptorViewModel : Observable
    {
        // ── MessageType ────────────────────────────────────────
        private LoginInterceptorEnum _messageType;
        public LoginInterceptorEnum MessageType
        {
            get { return _messageType; }
            set
            {
                _messageType = value;
                OnPropertyChanged("MessageType");
            }
        }

        // ── IsLoading ──────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        // ── ErrorMessage ───────────────────────────────────────
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        // ── Callback para o DialogService fechar o dialog ──────
        public Action OnLoginSuccess { get; set; }

        //***************************************************************
        // Services
        //***************************************************************
        private readonly INavigator _navigator;
        private readonly IUserContext _userContext;

        // ── Construtor ─────────────────────────────────────────
        public LoginInterceptorViewModel(
            LoginInterceptorEnum messageType,
            INavigator navigator,
            IUserContext userContext)
        {
            _messageType = messageType;
            _navigator = navigator;
            _userContext = userContext;
        }

        // ── Command de Login ───────────────────────────────────
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(async () =>
                    {
                        IsLoading = true;
                        ErrorMessage = null;

                        var success = await _userContext.LoginAsync();

                        IsLoading = false;

                        if (success)
                            OnLoginSuccess?.Invoke();
                        else
                            ErrorMessage = "Login cancelado. Tente novamente.";
                    });
                }
                return _loginCommand;
            }
        }
    }
}