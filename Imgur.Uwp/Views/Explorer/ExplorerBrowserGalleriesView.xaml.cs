using Imgur.Contracts;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
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

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Explorer
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ExplorerBrowserGalleriesView : Page
    {
        public ExplorerBrowserGalleriesView()
        {
            this.InitializeComponent();
        }

        public ExplorerBrowserGalleriesViewModel ViewModel => (ExplorerBrowserGalleriesViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as ExplorerBrowserGalleriesViewModel;
            await ViewModel.InitializeAsync();

            App.Services.GetRequiredService<IShareService>().Initialize();
        }
    }
}
