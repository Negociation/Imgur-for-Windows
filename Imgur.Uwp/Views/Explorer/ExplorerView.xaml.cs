using Imgur.Contracts;
using Imgur.Uwp.Converters;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Extensions;
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
        // ScrollViewer interno do AdaptiveGridView (substitui o antigo ContentScrollView,
        // removido junto com o ScrollViewer externo que quebrava a virtualização de UI).
        private ScrollViewer _gridScrollViewer;

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

            // Como a página usa NavigationCacheMode.Enabled, o Loaded do AdaptiveGridView
            // só dispara na primeira navegação. Em navegações seguintes a árvore visual já
            // existe, então garantimos aqui que a referência ao ScrollViewer interno está presa.
            HookGridScrollViewer();
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

        // Disparado pelo Loaded do AdaptiveGridView (ContentRepeaterControl) na XAML.
        private void ContentRepeaterControl_Loaded(object sender, RoutedEventArgs e)
        {
            HookGridScrollViewer();
        }

        // Busca o ScrollViewer interno gerado pelo template padrão do AdaptiveGridView/GridView
        // e assina o ViewChanged nele, no lugar do antigo ContentScrollView externo.
        private void HookGridScrollViewer()
        {
            if (_gridScrollViewer != null) return;

            _gridScrollViewer = ContentRepeaterControl.FindDescendant<ScrollViewer>();
            if (_gridScrollViewer != null)
            {
                _gridScrollViewer.ViewChanged += ContentScrollView_ViewChanged;
            }
            else
            {
                Debug.WriteLine("ScrollViewer interno do ContentRepeaterControl ainda não disponível.");
            }
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }
        private void ResetScrollOffset()
        {
            if (_gridScrollViewer != null && _gridScrollViewer.VerticalOffset > 0)
            {
                _gridScrollViewer.ChangeView(0, 0, null);
            }
        }
        private void PullContainerGrid_RefreshInvoked(DependencyObject sender, object args)
        {
            //Reload Refresh
            ViewModel.RetrieveGalleryContentCommand.Execute(null);
        }
        private void ContentScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

            if (_gridScrollViewer.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
            {
                ViewModel.CanScrollToTop = true;
            }
            else if (_gridScrollViewer.VerticalOffset == 0 && ViewModel.CanScrollToTop)
            {
                ViewModel.CanScrollToTop = false;
            }
        }
    }
}
