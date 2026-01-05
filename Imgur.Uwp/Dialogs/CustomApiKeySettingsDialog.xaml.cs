using Imgur.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class CustomApiKeySettingsDialog : ContentDialog
    {
        private SettingsViewModel _vm => DataContext as SettingsViewModel;

        public CustomApiKeySettingsDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(_vm.CustomAppId != ClientIdTextBox.Text || _vm.CustomClientSecret != ClientSecretTextBox.Text)
            {
                _vm.CustomAppId = ClientIdTextBox.Text;
                _vm.CustomClientSecret = ClientSecretTextBox.Text;
                if (_vm.RestartAppCommand?.CanExecute(null) == true)
                {
                    _vm.RestartAppCommand.Execute(null);
                }
            }

        }

        private void Close(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
