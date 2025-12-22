using Imgur.Contracts;
using Imgur.ViewModels.Media;
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

namespace Imgur.Uwp.Views.Media
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MediaView : Page
    {
        public MediaView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        public MediaViewModel ViewModel => (MediaViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as MediaViewModel;
            await ViewModel.InitializeAsync();

            App.Services.GetRequiredService<IShareService>().Initialize();
        }


        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }

        private void ResetScrollOffset()
        {
            if (RootScrollView.VerticalOffset > 0)
            {
                this.RootScrollView.ChangeView(0, 0, null);
            }
        }

        private void RootScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (this.RootScrollView.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = true;
                }
                else if (this.RootScrollView.VerticalOffset == 0 && ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = false;
                }
            }
            catch
            {
                Debug.WriteLine("Erro ScrollView ViewChanged");
            }
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Fix to use cascade content
            try
            {
                if (VisualStateManager.GetVisualStateGroups(this.RootGrid).First().CurrentState.Name == "Extended")
                {
                    RootScrollGrid.MaxHeight = RootGrid.ActualHeight;
                }
                else
                {
                    RootScrollGrid.MaxHeight = double.PositiveInfinity;
                }
            }
            catch
            {
                Debug.WriteLine("Erro RootGrid SizeChanged");

            }
        }


        private void ScrollToComments_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var transform = CommentsPlacement.TransformToVisual((UIElement)RootScrollView.Content);
                var position = transform.TransformPoint(new Point(0, 0));
                RootScrollView.ChangeView(null, position.Y, null);
            }
            catch
            {
                Debug.WriteLine("Erro ScrollView To Comments");
            }
        }
    }
}
