
using Imgur.Contracts;
using Imgur.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.ViewModels.FileUpload
{
    public class ShareTargetViewModel: Observable
    {


        //-- Uploading Status
        private bool _isUploading;

        public bool IsUploading
        {
            get { return _isUploading; }
            set
            {
                _isUploading = value;
                OnPropertyChanged("IsUploading");
            }
        }


        //-- Upload Success
        private bool _isCompleted;

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                OnPropertyChanged("IsCompleted");
            }
        }

        private readonly IUserContext _userContext;

        public bool IsAuthenticated => _userContext.IsAuthenticated;
        public bool IsNotAuthenticated => !_userContext.IsAuthenticated;

        private bool _isInitialized;

        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                OnPropertyChanged("IsInitialized");
            }
        }

        public ShareTargetViewModel(IUserContext userContext)
        {
            IsInitialized = false;
            _userContext = userContext;
        }
    }
}
