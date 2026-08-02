using Imgur.Contracts;
using Imgur.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Accounts
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class AccountView : Page
    {
        public AccountViewModel ViewModel => (AccountViewModel)this.DataContext;

        public AccountView()
        {
            this.InitializeComponent();
            this.LoadDesignAdapters();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as AccountViewModel;

            await ViewModel.InitializeAsync();

            App.Services.GetRequiredService<IShareService>().Initialize();

            // Aplica o layout mobile/xbox correto ao navegar
            ApplyMobileLayout();

            UpdateThumbailsState();

            UpdateCommentStyle(Window.Current.Bounds.Width);
        }

        private void LoadDesignAdapters()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                Window.Current.SizeChanged += MobileWindow_SizeChanged;
            }
        }

        private async void ApplyMobileLayout()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() ||
                App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                await Task.Delay(100);
                ApplyMobileCompactSettingsAlign(Window.Current.Bounds.Width);
                ApplyLeaveButtonMobileState(Window.Current.Bounds.Width);
            }
        }

        private async void MobileWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            await Task.Delay(100);
            ApplyMobileCompactSettingsAlign(e.Size.Width);
            ApplyLeaveButtonMobileState(e.Size.Width);
        }

        /// <summary>
        /// LeaveButtonMobileInlineState: 800–1099.
        /// LeaveButtonContinuumWideState: ≥1100 (top PC 50 − 20 = 30).
        /// </summary>
        private void ApplyLeaveButtonMobileState(double width)
        {
            if (width >= 800 && width < 1100)
                VisualStateManager.GoToState(this, "LeaveButtonMobileInlineState", false);
            else if (width >= 1100)
                VisualStateManager.GoToState(this, "LeaveButtonContinuumWideState", false);
            else
                VisualStateManager.GoToState(this, "LeaveButtonDefault", false);
        }

        /// <summary>
        /// CompactInline (500–800): no mobile/continuum alinha settings ao voltar.
        /// No PC essa faixa mantém Top 42 via AdaptiveTrigger.
        /// </summary>
        private void ApplyMobileCompactSettingsAlign(double width)
        {
            if (width >= 500 && width < 800)
                AccountSettingsButton.Margin = new Thickness(0, 17, 12, 0);
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

        // ── Scroll ────────────────────────────────────────────

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            bool canScroll = sv.VerticalOffset > 0;
            if (ViewModel != null && canScroll != ViewModel.CanScrollToTop)
                ViewModel.CanScrollToTop = canScroll;

            App.Services.GetRequiredService<INavigator>()
                        .ReportScrollOffset(sv.VerticalOffset);
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            MainScrollViewer?.ChangeView(0, 0, null);
        }

        // ── Pivot ─────────────────────────────────────────────

        private void AccountPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var idx = AccountPivot.SelectedIndex;
            BottomBarPosts.Visibility     = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
            BottomBarFavorites.Visibility = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
            BottomBarComments.Visibility  = idx == 2 ? Visibility.Visible : Visibility.Collapsed;

            // Volta ao topo ao trocar de aba
            MainScrollViewer?.ChangeView(0, 0, null, true);
            if (ViewModel != null)
                ViewModel.CanScrollToTop = false;
        }

        private void AccountViewPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCommentStyle(e.NewSize.Width);
        }

        private void UpdateCommentStyle(double width)
        {
            if (width >= 800)
            {
                CommentsRepeaterControl.ItemContainerStyle =
                    (Style)Resources["CommentDesktopStyle"];
            }
            else
            {
                CommentsRepeaterControl.ItemContainerStyle =
                    (Style)Resources["CommentMobileStyle"];
            }
        }
    }
}
