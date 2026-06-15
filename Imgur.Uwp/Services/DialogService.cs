using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Factories;
using Imgur.Models;
using Imgur.Uwp.Dialogs;
using Imgur.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private IUploadFileVmFactory _uploadFileVmFactory;
        private IUploadInterceptorVmFactory _uploadInterceptorVmFactory;
        public DialogService(
            IEmbedVmFactory embedFactory,
            ILoginInterceptorVmFactory loginInterceptorVmFactory,
            IUploadFileVmFactory uploadFileVmFactory,
            IUploadInterceptorVmFactory uploadInterceptorVmFactory
            ) {
            _embedFactory = embedFactory;
            _loginInterceptorVmFactory = loginInterceptorVmFactory;
            _uploadFileVmFactory = uploadFileVmFactory;
            _uploadInterceptorVmFactory = uploadInterceptorVmFactory;
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
            var dialog = new LoginInterceptorDialog { DataContext = loginInterceptorVm };

            var loginSucceeded = false;
            loginInterceptorVm.OnLoginSuccess = () =>
            {
                loginSucceeded = true;
                dialog.Hide();
            };

            await dialog.ShowAsync();
            return loginSucceeded;
        }

        public async Task ShowUploadInterceptorDialog()
        {
            var uploadInterceptorVm = _uploadInterceptorVmFactory.GetUploadInterceptorViewModel();
            var dialog = new UploadInterceptorDialog { DataContext = uploadInterceptorVm };

            uploadInterceptorVm.OnSettingInvoked = () =>
            {
                dialog.Hide();
            };

            await dialog.ShowAsync();
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

        public Task<bool?> ShowUploadDialogAsync() => ShowUploadDialogAsync(null);
        /*
        public async Task<bool?> ShowUploadDialogAsync(IList<SelectedFile> preSelectedFiles = null)
        {
            var uploadVm = _uploadFileVmFactory.GetUploadFileViewModel();
            var dialog = new UploadDialog { DataContext = uploadVm };

            // Popula arquivos antes de subscrever
            if (preSelectedFiles != null && preSelectedFiles.Count > 0)
            {
                foreach (var file in preSelectedFiles)
                    uploadVm.SelectedFiles.Add(file);
                uploadVm.NotifyFilesChanged();
            }

            // Subscreve APÓS popular — lê o estado atual correto
            dialog.SubscribeViewModel();

            var uploadSucceeded = false;
            uploadVm.OnUploadSuccess = () =>
            {
                uploadSucceeded = true;
                dialog.Hide();
            };

            await dialog.ShowAsync();
            return uploadSucceeded;
        }
        */

        public async Task<bool?> ShowUploadDialogAsync(IList<SelectedFile> preSelectedFiles = null, Action onUploadStarted = null, Action onUploadCompleted = null)
        {
            var uploadVm = _uploadFileVmFactory.GetUploadFileViewModel();
            var dialog = new UploadDialog { DataContext = uploadVm };
            var uploadCompletion = new TaskCompletionSource<bool>();

            if (preSelectedFiles != null && preSelectedFiles.Count > 0)
            {
                foreach (var file in preSelectedFiles)
                    uploadVm.SelectedFiles.Add(file);
                uploadVm.NotifyFilesChanged();
            }

            dialog.SubscribeViewModel();

            uploadVm.OnUploadStarted = () =>
            {
                dialog.Hide();
                onUploadStarted?.Invoke();
            };

            uploadVm.OnUploadSuccess = () =>
            {
                uploadCompletion.TrySetResult(true);
                onUploadCompleted?.Invoke();
            };

            uploadVm.OnCancel = () =>
            {
                uploadCompletion.TrySetResult(false);
                dialog.Hide();
            };

            var result = await dialog.ShowAsync();
            Debug.WriteLine($"Dialog result = {result}");
            if (result == ContentDialogResult.Secondary)
            {
                // Usuário clicou em Cancelar
                uploadCompletion.TrySetResult(false);
            }
            return await uploadCompletion.Task;
        }
    }
}
