using Imgur.ViewModels.FileUpload;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Caixa de Diálogo de Conteúdo está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Dialogs
{
    public sealed partial class UploadDialog : ContentDialog
    {
        public UploadFileViewModel ViewModel => (UploadFileViewModel)this.DataContext;

        public UploadDialog()
        {
            this.InitializeComponent();

            //Fix for Mobile
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile"){
                this.FullSizeDesired = true;
            }

            this.PrimaryButtonClick += async (sender, args) =>
            {
                args.Cancel = true;
                ViewModel.UploadCommand.Execute(null);
            };
        }

        public void SubscribeViewModel()
        {
            this.IsPrimaryButtonEnabled = ViewModel.IsReadyForUpload;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine(e.PropertyName);

            if (e.PropertyName == nameof(ViewModel.IsReadyForUpload))
            {
                this.IsPrimaryButtonEnabled = ViewModel.IsReadyForUpload;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.OnCancel?.Invoke();
        }
    }
}
