using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Models;
using Imgur.Uwp.Dialogs;
using Imgur.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Services
{
    public class DialogService : IDialogService
    {
        private IEmbedVmFactory _embedFactory;
        private ILoginInterceptorVmFactory _loginInterceptorVmFactory;

        public DialogService(IEmbedVmFactory embedFactory, ILoginInterceptorVmFactory loginInterceptorVmFactory)
        {
            _embedFactory = embedFactory;
            _loginInterceptorVmFactory = loginInterceptorVmFactory;
        }

        public async Task<bool?> ShowEmbedDialogAsync(Media media)
        {
            var embedVm = _embedFactory.GetEmbedViewModel(media);

            var dialog = new EmbedDialog
            {
                DataContext = embedVm
            };

            var resultado = await dialog.ShowAsync();
            return resultado == ContentDialogResult.Primary;
        }

        public async Task<bool?> ShowLoginInterceptorDialog(LoginInterceptorEnum loginMessage)
        {
            var loginInterceptorVm = _loginInterceptorVmFactory.getLoginInterceptorViewModel(loginMessage);

            var dialog = new LoginInterceptorDialog {
                DataContext = loginInterceptorVm
            };

            var resultado = await dialog.ShowAsync();
            return resultado == ContentDialogResult.Primary;

        }

        public async Task<bool?> ShowCustomApiKeyDialog(SettingsViewModel settingsVm)
        {
            
            var dialog = new CustomApiKeySettingsDialog
            {
                DataContext = settingsVm
            };

            var resultado = await dialog.ShowAsync();
            return resultado == ContentDialogResult.Primary;
            

        }
    }
}
