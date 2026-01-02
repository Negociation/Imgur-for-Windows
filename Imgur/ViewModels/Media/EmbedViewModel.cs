using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Imgur.ViewModels.Media
{
    public class EmbedViewModel: Observable
    {

        //***************************************************************
        // View Params
        //***************************************************************
        //-- Current Media to Show
        private Imgur.Models.Media _currentMedia;

        public Imgur.Models.Media CurrentMedia
        {
            get { return _currentMedia; }
            set
            {
                _currentMedia = value;
                OnPropertyChanged("CurrentMedia");
            }
        }

        //***************************************************************
        // Services
        //***************************************************************

        //-- Service para Clipboard
        private IClipboardService _clipboardService;

        //-- Service para Envio de Notificações
        private readonly IAppNotificationService _appNotification;

        //***************************************************************
        // Constructors e Initializers
        //***************************************************************

        //-- Constructor
        public EmbedViewModel(
            Models.Media m,
            IClipboardService clipboardService,
            IAppNotificationService appNotification
            )
        {
            CurrentMedia = m;
            _clipboardService = clipboardService;
            _appNotification = appNotification;
        }


        //***************************************************************
        // Commands
        //***************************************************************

        //-- Command para Copiar para o Clipboard
        private ICommand _copy;

        public ICommand Copy
        {
            get
            {
                if (_copy == null)
                {
                    _copy = new RelayCommand(async () => {
                        if (await this._clipboardService.GetTextAsync() != this.CurrentMedia.Embed)
                        {
                            this._clipboardService.SetText(this.CurrentMedia.Embed);
                            NotificationViewModel notification = new NotificationViewModel();
                            notification.Message = "Embed Content Copied to Clipboard";
                            this._appNotification.AddNotification(notification);
                        }

                    });
                }
                return _copy;
            }
        }

    }
}
