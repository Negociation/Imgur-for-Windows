using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Imgur.ViewModels.FileUpload
{
    public class UploadInterceptorViewModel: Observable
    {
        private readonly INavigator _navigator;
        private readonly ILocalSettings _localSettings;
        private readonly IClipboardService _clipboardService;
        private readonly IAppNotificationService _appNotificationService;
        public string AppRedirectCallback
        {
            get => _localSettings.Get<string>(LocalSettingsConstants.AppRedirectCallback);
        }

        public UploadInterceptorViewModel(INavigator navigator, ILocalSettings localSettings, IClipboardService clipboardService, IAppNotificationService notificationService)
        {
            this._navigator = navigator;
            this._localSettings = localSettings;
            this._clipboardService = clipboardService;
            this._appNotificationService = notificationService;
        }

        public Action OnSettingInvoked { get; set; }

        // ── Command de Abrir Settings ───────────────────────────────────
        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                if (_openSettingsCommand == null)
                {
                    _openSettingsCommand = new RelayCommand(() =>
                    {
                        OnSettingInvoked?.Invoke();
                        this._navigator.Navigate("settings");
                    });
                }
                return _openSettingsCommand;
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
                    _copy = new RelayCommand( () => {
                            this._clipboardService.SetText(this.AppRedirectCallback);
                            NotificationViewModel notification = new NotificationViewModel();
                            notification.Message = "ApiRedirectInAppNotificationDesc";
                            this._appNotificationService.AddNotification(notification);
                    });
                }
                return _copy;
            }
        }
    }
}
