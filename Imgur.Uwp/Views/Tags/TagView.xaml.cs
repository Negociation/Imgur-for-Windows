using Imgur.Contracts;
using Imgur.ViewModels.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Views.Tags
{
    public sealed partial class TagView : Page
    {
        public TagViewModel ViewModel => (TagViewModel)this.DataContext;

        private const double COMPACT_THRESHOLD = 150;

        public TagView()
        {
            this.InitializeComponent();
            this.LoadDesignAdapters();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as TagViewModel;

            App.Services.GetRequiredService<IShareService>().Initialize();
            UpdateThumbailsState();

            // Aplica o layout mobile/xbox correto ao navegar
            ApplyMobileLayout();
        }

        private void LoadDesignAdapters()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                Window.Current.SizeChanged += MobileWindow_SizeChanged;
            }
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



        private async void ApplyMobileLayout()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                // Aguarda a página estar renderizada
                await Task.Delay(100);

                if (Window.Current.Bounds.Width >= 800)
                {
                    VisualStateManager.GoToState(this, "LeaveButtonMobileInlineState", false);
                }
            }
        }

        private async void MobileWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 800)
            {
                await Task.Delay(100);
                VisualStateManager.GoToState(this, "LeaveButtonMobileInlineState", false);
            }
        }


        private void MainScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            AnimateHeader(e.NextView.VerticalOffset);
        }

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            AnimateHeader(scrollViewer.VerticalOffset);
        }

        private void AnimateHeader(double offset)
        {
            // Progresso da animação (0 a 1)
            double progress = Math.Min(offset / COMPACT_THRESHOLD, 1.0);

            // Verifica se está no final
            var scrollViewer = MainScrollViewer;
            bool isAtBottom = false;

            if (scrollViewer != null && scrollViewer.ScrollableHeight > 0)
            {
                double distanceFromBottom = scrollViewer.ScrollableHeight - offset;
                isAtBottom = distanceFromBottom < 10;
            }

            // === STICK OPTIONS BAR ===
            if (StickOptionsBar != null)
            {
                const double MIN_SCROLL_TO_SHOW = 60;

                if (offset > MIN_SCROLL_TO_SHOW)
                {
                    StickOptionsBar.Visibility = Visibility.Visible;

                    if (isAtBottom)
                    {
                        StickOptionsBar.Opacity = 1.0;
                    }
                    else
                    {
                        double minOpacity = 0.6;
                        StickOptionsBar.Opacity = Math.Max(progress, minOpacity);
                    }
                }
                else
                {
                    StickOptionsBar.Visibility = Visibility.Collapsed;
                    StickOptionsBar.Opacity = 0;
                }
            }

            // === FADE DOS POSTS PREVIEW ===
            if (PostsPreview != null)
            {
                PostsPreview.Opacity = 1 - progress;
            }

            if (PostCount != null)
            {
                PostCount.Opacity = 1 - progress;
            }

            if (PostCountLabel != null)
            {
                PostCountLabel.Opacity = 1 - progress;
            }

            // === ENCOLHER TÍTULO E DIMINUIR OPACITY ===
            if (TitleScale != null && ExpandedTitle != null)
            {
                // Scale: de 1.0 (100%) até 0.3 (30%)
                double targetScale = 0.3;
                double scale = 1.0 - (progress * (1.0 - targetScale));
                TitleScale.ScaleX = scale;
                TitleScale.ScaleY = scale;

                // Opacity: de 1.0 até 0 (desaparece completamente)
                ExpandedTitle.Opacity = 1.0 - progress;
            }

            // Reseta o translate se existir
            if (TitleTranslate != null)
            {
                TitleTranslate.X = 0;
                TitleTranslate.Y = 0;
            }
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }

        private void ResetScrollOffset()
        {
            if (MainScrollViewer.VerticalOffset > 0)
            {
                this.MainScrollViewer.ChangeView(0, 0, null);
            }
        }

    }
}