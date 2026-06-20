using Imgur.Contracts;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
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
    public sealed partial class CommentDialog : ContentDialog
    {
        public MediaViewModel ViewModel => (MediaViewModel)this.DataContext;

        public CommentDialog()
        {
            this.InitializeComponent();
            ConfigureFullSize();

        }

        private void ConfigureFullSize()
        {
            var sysService = App.Services.GetRequiredService<ISystemInfoProvider>();

            var isMobile = sysService.IsMobile();
            var continuumMode = sysService.IsContinuum();
            // Só aplica FullSizeDesired se mobile E não for Continuum
            if (isMobile && !continuumMode)
            {
                this.FullSizeDesired = true;
            }
        }
        private void Close(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
