using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.ViewModels.Explorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Views.Explorer
{
    public sealed partial class ExplorerView : Page
    {
        // ScrollViewer interno do AdaptiveGridView (substitui o antigo ContentScrollView,
        // removido junto com o ScrollViewer externo que quebrava a virtualização de UI).
        private ScrollViewer _gridScrollViewer;
        private bool _gallerySwipeActive;
        private bool _gallerySwipeAnimating;

        public ExplorerView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.DataContext = App.Services.GetRequiredService<ExplorerViewModel>();

            // Os itens (GridViewItem) geram a manipulação; o evento sobe até o GridView.
            ContentRepeaterControl.AddHandler(
                ManipulationDeltaEvent,
                new ManipulationDeltaEventHandler(OnGallerySwipeDelta),
                true);
            ContentRepeaterControl.AddHandler(
                ManipulationCompletedEvent,
                new ManipulationCompletedEventHandler(OnGallerySwipeCompleted),
                true);
            ContentRepeaterControl.AddHandler(
                ManipulationInertiaStartingEvent,
                new ManipulationInertiaStartingEventHandler(OnGallerySwipeInertiaStarting),
                true);
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

        private void ContentRepeaterControl_Loaded(object sender, RoutedEventArgs e)
        {
            HookGridScrollViewer();
        }

        private void HookGridScrollViewer()
        {
            if (_gridScrollViewer != null)
                return;

            _gridScrollViewer = ContentRepeaterControl.FindDescendant<ScrollViewer>();
            if (_gridScrollViewer != null)
                _gridScrollViewer.ViewChanged += ContentScrollView_ViewChanged;
            else
                Debug.WriteLine("ScrollViewer interno do ContentRepeaterControl ainda não disponível.");
        }

        private bool CanSwipeGallerySections()
        {
            return GallerySelector.Visibility == Visibility.Visible
                && !ViewModel.Loading
                && !_gallerySwipeAnimating;
        }

        // Empurra o container junto com o dedo (só depois de o gesto ser claramente horizontal).
        private void OnGallerySwipeDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch || !CanSwipeGallerySections())
                return;

            var dx = e.Cumulative.Translation.X;
            var dy = e.Cumulative.Translation.Y;

            if (!_gallerySwipeActive)
            {
                if (Math.Abs(dx) < 12)
                    return;
                if (Math.Abs(dx) < Math.Abs(dy) * 1.2)
                    return;

                _gallerySwipeActive = true;
            }

            // Resistência leve nas pontas para não “voar” a tela inteira.
            var width = Math.Max(PullContainerGrid.ActualWidth, 1);
            GallerySwipeTransform.X = Math.Max(-width, Math.Min(width, dx));
            e.Handled = true;
        }

        private void OnGallerySwipeInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            if (!_gallerySwipeActive)
                return;

            // Sem inércia horizontal: o Completed decide se troca a seção ou volta.
            e.TranslationBehavior.DesiredDeceleration = double.MaxValue;
            e.Handled = true;
        }

        private async void OnGallerySwipeCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!_gallerySwipeActive)
                return;

            _gallerySwipeActive = false;

            if (e.PointerDeviceType != PointerDeviceType.Touch || GallerySelector.Visibility != Visibility.Visible)
            {
                await AnimateGallerySwipeAsync(0);
                return;
            }

            var direction = SwipeGesture.Classify(e.Cumulative.Translation.X, e.Velocities.Linear.X);
            if (direction == SwipeDirection.None)
            {
                await AnimateGallerySwipeAsync(0);
                return;
            }

            var width = Math.Max(PullContainerGrid.ActualWidth, 1);
            var exitX = direction == SwipeDirection.Left ? -width : width;
            await AnimateGallerySwipeAsync(exitX);

            ViewModel.SelectedSectionIndex = SwipeGesture.CycleIndex(
                ViewModel.SelectedSectionIndex, ViewModel.Sections.Count, direction);

            // Entra pelo lado oposto e aterrissa em 0 (efeito de página).
            GallerySwipeTransform.X = -exitX;
            await AnimateGallerySwipeAsync(0);
            e.Handled = true;
        }

        private Task AnimateGallerySwipeAsync(double toX)
        {
            var tcs = new TaskCompletionSource<bool>();
            _gallerySwipeAnimating = true;

            var animation = new DoubleAnimation
            {
                To = toX,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(animation, GallerySwipeTransform);
            Storyboard.SetTargetProperty(animation, "X");

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Completed += (s, e) =>
            {
                GallerySwipeTransform.X = toX;
                _gallerySwipeAnimating = false;
                tcs.TrySetResult(true);
            };
            sb.Begin();
            return tcs.Task;
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }

        private void ResetScrollOffset()
        {
            if (_gridScrollViewer != null && _gridScrollViewer.VerticalOffset > 0)
                _gridScrollViewer.ChangeView(0, 0, null);
        }

        private void PullContainerGrid_RefreshInvoked(DependencyObject sender, object args)
        {
            ViewModel.RetrieveGalleryContentCommand.Execute(null);
        }

        private void ContentScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_gridScrollViewer.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
                ViewModel.CanScrollToTop = true;
            else if (_gridScrollViewer.VerticalOffset == 0 && ViewModel.CanScrollToTop)
                ViewModel.CanScrollToTop = false;
        }
    }
}
