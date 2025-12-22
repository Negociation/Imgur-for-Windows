using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public class AccountViewModel : Observable
    {

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

        //-- Usuario Atual Autenticado
        public bool IsAuthenticated => _userContext.IsAuthenticated;

        //***************************************************************
        // Services 
        //***************************************************************


        //-- Service para Dialogos 
        private readonly IDialogService _dialogService;

        //-- UserContext
        private readonly IUserContext _userContext;


        public AccountViewModel(UserAccount account, IUserContext userContext, IDialogService dialogService)
        {
            this.UserAccountInfo = account;
            this._userContext = userContext;
            this._dialogService = dialogService;
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

    }
}
