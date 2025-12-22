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
                this.DataContext = viewModel;
            }


            await ViewModel.InitializeAsync();
            UpdateThumbailsState();
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
           //ResetScrollOffset();
        }

        private void UpdateThumbailsState()
        {
            VisualStateManager.GoToState(this, nameof(RandomMediaGrid_Medium), false);

            /*
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
            */
        }

    }
}
