using Imgur.Factories;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class ExplorerSearchView : Page
    {
        public ExplorerSearchViewModel ViewModel => (ExplorerSearchViewModel)this.DataContext;


        public ExplorerSearchView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            if (ViewModel == null)
            {
                var vmFactory = App.Services.GetRequiredService<IExplorerSearchVmFactory>();
                this.DataContext = vmFactory.getExplorerViewModel();
            }

        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e?.Parameter is ExplorerSearchViewModel viewModel)
            {
                // Modo Search - recebeu um ViewModel com query
                Debug.WriteLine($"Search mode: '{viewModel.SearchQuery}'");
                this.DataContext = viewModel;
            }
            else
            {
                // Modo Explore - não recebeu ViewModel, precisa criar um novo
                Debug.WriteLine("Explore mode: creating new ViewModel");

                var vmFactory = App.Services.GetRequiredService<IExplorerSearchVmFactory>();
                var exploreVm = vmFactory.getExplorerViewModel();
                this.DataContext = exploreVm;
            }

            await ViewModel.InitializeAsync();
            UpdateThumbailsState();
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
           ResetScrollOffset();
        }


        private void ResetScrollOffset()
        {
            if (ViewModel.IsExplorerMode)
            {
                if (ExplorerContentScrollView.VerticalOffset > 0)
                {
                    this.ExplorerContentScrollView.ChangeView(0, 0, null);
                }
            }

            if (ViewModel.IsSearchMode)
            {
                if (SearchContentScrollView.VerticalOffset > 0)
                {
                    this.SearchContentScrollView.ChangeView(0, 0, null);
                }
            }
        }

        private void ExplorerContentScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ViewModel.IsExplorerMode)
            {
                if (this.ExplorerContentScrollView.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = true;
                }
                else if (this.ExplorerContentScrollView.VerticalOffset == 0 && ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = false;
                }
            }
        }

        private void UpdateThumbailsState()
        {
            if (ViewModel.IsExplorerMode)
            {
                switch (ViewModel.ThumbSize)
                {
                    case 0:
                        VisualStateManager.GoToState(this, nameof(RandomMediaGrid_Small), false);
                        break;
                    case 1:
                        VisualStateManager.GoToState(this, nameof(RandomMediaGrid_Medium), false);
                        break;
                    case 2:
                        VisualStateManager.GoToState(this, nameof(RandomMediaGrid_Larger), false);
                        break;
                }
            }
        }

        private void SearchContentScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ViewModel.IsSearchMode)
            {
                if (this.SearchContentScrollView.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = true;
                }
                else if (this.SearchContentScrollView.VerticalOffset == 0 && ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = false;
                }
            }
        }
    }
}
