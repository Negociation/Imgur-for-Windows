using Imgur.Contracts;
using Imgur.Uwp.Converters;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Explorer
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ExplorerView : Page
    {

        public ExplorerView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.DataContext = App.Services.GetRequiredService<ExplorerViewModel>();
        }

        public ExplorerViewModel ViewModel => (ExplorerViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
            UpdateThumbailsState();
            App.Services.GetRequiredService<IShareService>().Initialize();
        }

        private void UpdateThumbailsState()
        {
            switch (ViewModel.ThumbSize)
            {
                case 0:
                    VisualStateManager.GoToState(this, nameof(MediaGrid_Small), false);
                    break;
                case 1:
                    VisualStateManager.GoToState(this, nameof(MediaGrid_Medium), false);
                    break;
                case 2:
                    VisualStateManager.GoToState(this, nameof(MediaGrid_Larger), false);
                    break;
            }
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }

        private void ResetScrollOffset()
        {
            if (ContentScrollView.VerticalOffset > 0)
            {
                this.ContentScrollView.ChangeView(0, 0, null);
            }  
        }


        private void PullContainerGrid_RefreshInvoked(DependencyObject sender, object args)
        {
            //Reload Refresh
            ViewModel.RetrieveGalleryContentCommand.Execute(null);
        }

        private void ContentScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (this.ContentScrollView.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
            {
                ViewModel.CanScrollToTop = true;
            }
            else if (this.ContentScrollView.VerticalOffset == 0 && ViewModel.CanScrollToTop)
            {
                ViewModel.CanScrollToTop = false;
            }
        }
    }

}

