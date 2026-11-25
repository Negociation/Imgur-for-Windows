using Imgur.Contracts;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Views.Accounts
{
    public sealed partial class EditProfileView : Page
    {
        private const double CompactThreshold = 150;
        private const double MinScrollToShowSticky = 60;
        private const double CompactMinWidth = 500;
        private const double AccountLeftPull = -5;

        private EditProfileViewModel ViewModel => DataContext as EditProfileViewModel;

        public EditProfileView()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<EditProfileViewModel>();
            this.SizeChanged += (s, e) => ApplyLayoutChrome();
            this.Loaded += (s, e) => ApplyLayoutChrome();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void ApplyLayoutChrome()
        {
            bool isCompactOrWider = Window.Current.Bounds.Width >= CompactMinWidth;
            bool isMobile = App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile();

            string chromeState = !isCompactOrWider ? nameof(ChromeMinimal)
                               : isMobile ? nameof(ChromeMobile)
                               : nameof(ChromeDesktop);
            VisualStateManager.GoToState(this, chromeState, false);

            if (isCompactOrWider)
                AnchorToWindowTop();
        }

        // A altura acima do MainFrame varia (search/brand/padding da Shell por largura e rota),
        // então o pull negativo é medido em vez de fixo como na AccountView.
        private void AnchorToWindowTop()
        {
            if (!(Window.Current.Content is UIElement windowRoot))
                return;

            double offsetFromWindowTop = TransformToVisual(windowRoot).TransformPoint(new Point(0, 0)).Y;
            if (Math.Abs(offsetFromWindowTop) < 0.5)
                return;

            Margin = new Thickness(AccountLeftPull, Margin.Top - offsetFromWindowTop, 0, 0);
        }

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            AnimateHeader(sv.VerticalOffset);

            App.Services.GetRequiredService<INavigator>()
                        .ReportScrollOffset(sv.VerticalOffset);
        }

        private void AnimateHeader(double offset)
        {
            double progress = Math.Min(offset / CompactThreshold, 1.0);

            bool isAtBottom = false;
            if (MainScrollViewer != null && MainScrollViewer.ScrollableHeight > 0)
            {
                double distanceFromBottom = MainScrollViewer.ScrollableHeight - offset;
                isAtBottom = distanceFromBottom < 10;
            }

            if (StickOptionsBar != null)
            {
                if (offset > MinScrollToShowSticky)
                {
                    StickOptionsBar.Visibility = Visibility.Visible;

                    if (isAtBottom)
                        StickOptionsBar.Opacity = 1.0;
                    else
                        StickOptionsBar.Opacity = Math.Max(progress, 0.6);
                }
                else
                {
                    StickOptionsBar.Visibility = Visibility.Collapsed;
                    StickOptionsBar.Opacity = 0;
                }
            }

            // TopBar / Save flutuantes somem quando a sticky assume o título
            bool showFloating = offset <= MinScrollToShowSticky;
            if (TopBar != null)
            {
                TopBar.Opacity = showFloating ? 1 : 0;
                TopBar.IsHitTestVisible = showFloating;
            }

            if (SaveShadow != null)
            {
                SaveShadow.Opacity = showFloating ? 1 : 0;
                SaveShadow.IsHitTestVisible = showFloating;
            }
        }
    }
}
