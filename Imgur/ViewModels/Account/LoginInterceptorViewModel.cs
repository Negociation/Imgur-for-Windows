using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace Imgur.ViewModels.Account
{
    public class LoginInterceptorViewModel: Observable
    {

        //-- Message Type
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

        //-- Serviço de Navegação
        private readonly INavigator _navigator;

        public LoginInterceptorViewModel(LoginInterceptorEnum messageType, INavigator navigator)
        {
            _messageType = messageType;
            _navigator = navigator;
        }
    }
}
