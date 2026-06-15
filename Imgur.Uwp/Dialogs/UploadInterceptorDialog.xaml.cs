using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Caixa de Diálogo de Conteúdo está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Dialogs
{
    public sealed partial class UploadInterceptorDialog : ContentDialog
    {
        public UploadInterceptorDialog()
        {
            this.InitializeComponent();
            var resourceLoader = ResourceLoader.GetForCurrentView();

            var before = new Run { Text = resourceLoader.GetString("UploadInterceptorApiKeyInfoBefore") };
            var after = new Run { Text = resourceLoader.GetString("UploadInterceptorApiKeyInfoAfter") };
            var link = new Hyperlink { NavigateUri = new Uri("https://imgur.com/account/settings/apps") };
            link.Inlines.Add(new Run { Text = "https://imgur.com/account/settings/apps" });

            ApiKeyInfoText.Inlines.Insert(0, before);  
            ApiKeyInfoText.Inlines.Add(after);          
        }
    

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void Close(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
