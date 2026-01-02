using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Models;
using Imgur.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Imgur.Uwp.Services
{
    public class ClipboardService : IClipboardService
    {
        // -- Variaveis de Serviço
        private DataPackage dataPackage = new DataPackage();
        private bool _isMonitoring;
        private readonly Stack<string> _clipboardHistory;
        private bool _ignoringNextChange;

        //-- Services
        private readonly IAppNotificationService _notification;
        private readonly UrlHandlerService _urlHandler;
        private readonly IMediaVmFactory _mediaVmFactory;

        //-- Construtor
        public ClipboardService(
            IAppNotificationService notification,
            UrlHandlerService urlHandler,
            IMediaVmFactory mediaVmFactory
            )
        {
            this._notification = notification;
            this._urlHandler = urlHandler;
            this._mediaVmFactory = mediaVmFactory;
            this._clipboardHistory = new Stack<string>();
        }

        //-- Metodos
        public void SetText(string text)
        {
            // Copia interna então ignorar no event handler 
            _ignoringNextChange = true;

            //Setar no Clipboard
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);

            //Adicionar a Stack de Historico
            _clipboardHistory.Push(text);
        }

        public async Task<string> GetTextAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                return await dataPackageView.GetTextAsync();
            }
            return string.Empty;
        }

        public void StartMonitoring()
        {
            if (_isMonitoring) return;

            Clipboard.ContentChanged += OnClipboardContentChanged;
            _isMonitoring = true;

            // Verifica clipboard inicial ao iniciar
            _ = CheckCurrentClipboard();
        }

        public void StopMonitoring()
        {
            Clipboard.ContentChanged -= OnClipboardContentChanged;
            _isMonitoring = false;
        }

        private async void OnClipboardContentChanged(object sender, object e)
        {
            //Se foi mudança interna ignorar
            if (_ignoringNextChange)
            {
                _ignoringNextChange = false;
                return;
            }

            await CheckCurrentClipboard();
        }

        private async Task CheckCurrentClipboard()
        {
            try
            {
                var content = Clipboard.GetContent();

                if (!content.Contains(StandardDataFormats.Text))
                    return;

                string clipboardText = await content.GetTextAsync();

                // Verifica se é diferente do último processado
                if (_clipboardHistory.Count > 0 && _clipboardHistory.Peek() == clipboardText)
                    return;

                // Adiciona na stack
                _clipboardHistory.Push(clipboardText);

                // Tenta processar como URL do Imgur
                var urlResult = await _urlHandler.HandleAsync(clipboardText);

                if (urlResult.IsValid)
                {
                    this.convertAndShowNotification(urlResult);
                }
            }
            catch (Exception ex)
            {
                // Log silencioso
                System.Diagnostics.Debug.WriteLine($"Erro ao processar clipboard: {ex.Message}");
            }
        }

        private void convertAndShowNotification(ImgurUrl result)
        {
            switch (result.Type)
            {
                case ImgurUrlType.Album:
                    var galleryViewModel = this._mediaVmFactory.GetMediaViewModel(result.Data as Media);
                    _notification.AddMediaClipboardNotification(galleryViewModel);
                    break;
            }
        }
    }
}
