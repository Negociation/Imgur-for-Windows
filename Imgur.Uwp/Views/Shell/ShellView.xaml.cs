using Imgur.Contracts;
using Imgur.Uwp.Helpers;
using Imgur.Uwp.Views.Explorer;
using Imgur.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Imgur.Uwp.Views.Shell
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ShellView : Page
    {
        public ShellViewModel ViewModel => (ShellViewModel)this.DataContext;
        public bool IsTitleBarPresent { get; private set; }
        private InAppNotificationPresenter _notificationPresenter;

        public ShellView()
        {
            this.LoadDesignAdapters();
            this.InitializeComponent();
            // Como o Ambie: manter Shell vivo no RootFrame ao ir/voltar do fullscreen
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.DataContext = App.Services.GetRequiredService<ShellViewModel>();
            Loaded += Page_Loaded;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            _notificationPresenter = new InAppNotificationPresenter(ShellGrid);
        }

        private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Recieve Fullscreen Mode (Title Bar)
            if(e.PropertyName == nameof(ViewModel.IsFullScreenMode))
            {
                if (ViewModel.IsFullScreenMode)
                {
                    VisualStateManager.GoToState(this, "CompactHeaderFullScreenVisibleState", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "CompactHeaderDefaultVisibleState", true);
                }
            }           

            //Update NavigationView Selected Page when use internal navigation
            if (e.PropertyName == nameof(ViewModel.CurrentPageIndex))
            {
                if (ViewModel.CurrentPageIndex > -1)
                {
                    this.NavigationView.SelectedItem = (ViewModel.CurrentPageIndex < NavigationView.MenuItems.Count) ? NavigationView.MenuItems[ViewModel.CurrentPageIndex] : NavigationView.FooterMenuItems[ViewModel.CurrentPageIndex - NavigationView.MenuItems.Count];
                }
                else
                {
                    this.NavigationView.SelectedItem = -1;
                }
            }

            await Task.Delay(1000);
        }
        
        private void LoadDesignAdapters()
        {
            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() || App.Services.GetRequiredService<ISystemInfoProvider>().IsXbox())
            {
                IsTitleBarPresent = false;
                Window.Current.SizeChanged += MobileWindow_SizeChanged;
            }
            else
            {
                IsTitleBarPresent = true;
                Window.Current.SetTitleBar(PaneTitle);
                Window.Current.SizeChanged += DesktopWindow_SizeChanged;
            }
        }

        private void DesktopWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < 500)
            {
                VisualStateManager.GoToState(this, "CompactOverlayDesktopState", true);
            }
            if (e.Size.Width >= 800 && Window.Current.Bounds.Width <= 1200)
            {
                VisualStateManager.GoToState(this, "SearchDesktopInlineState", false);
            }
        }

        private void MobileWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 800)
            {
                VisualStateManager.GoToState(this, "SearchMobileInlineState", false);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.Frame = MainFrame;

            // Só inicializa/restaura na primeira entrada. Voltar do fullscreen (RootFrame)
            // não pode resetar o MainFrame para Explorer.
            if (e.NavigationMode == NavigationMode.New && MainFrame.Content == null)
            {
                if (!navigator.TryRestoreNavigationState())
                    MainFrame.Navigate(typeof(ExplorerView));
            }

            // Reassume a title bar ao voltar do player fullscreen
            if (IsTitleBarPresent)
                Window.Current.SetTitleBar(PaneTitle);

            navigator.ScrollOffsetChanged -= Navigator_ScrollOffsetChanged;
            navigator.FullScreenModeChanged -= Navigator_FullScreenModeChanged;
            navigator.ScrollOffsetChanged += Navigator_ScrollOffsetChanged;
            navigator.FullScreenModeChanged += Navigator_FullScreenModeChanged;

            _notificationPresenter?.Attach();
            ViewModel.Initialize();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _notificationPresenter?.Detach();
            base.OnNavigatingFrom(e);
        }

        private void Navigator_ScrollOffsetChanged(object sender, double offset)
        {
            var sysInfo = App.Services.GetRequiredService<ISystemInfoProvider>();
            bool isDesktop = !sysInfo.IsMobile() && !sysInfo.IsXbox();

            if (offset > 10)
            {
                // Barra acrílica apenas no PC
                if (isDesktop && AcrylicBarFullScrenBase.Visibility == Visibility.Collapsed)
                {
                    AcrylicBarFullScrenBase.Opacity = 0;
                    AcrylicBarFullScrenBase.Visibility = Visibility.Visible;

                    var sbIn = new Storyboard();
                    var fadeIn = new DoubleAnimation
                    {
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(220),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    Storyboard.SetTarget(fadeIn, AcrylicBarFullScrenBase);
                    Storyboard.SetTargetProperty(fadeIn, "Opacity");
                    sbIn.Children.Add(fadeIn);
                    sbIn.Begin();
                }

                // Fade-out do header compacto e da search box
                if (HeaderCompactContent.Opacity > 0)
                {
                    var sb = new Storyboard();

                    var fadeHeader = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromMilliseconds(200) };
                    Storyboard.SetTarget(fadeHeader, HeaderCompactContent);
                    Storyboard.SetTargetProperty(fadeHeader, "Opacity");

                    var fadeSearch = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromMilliseconds(200) };
                    Storyboard.SetTarget(fadeSearch, SearchBox);
                    Storyboard.SetTargetProperty(fadeSearch, "Opacity");

                    sb.Children.Add(fadeHeader);
                    sb.Children.Add(fadeSearch);
                    sb.Begin();
                }
            }
            else
            {
                // Fade-out da barra acrílica ao voltar ao topo (apenas PC)
                if (isDesktop && AcrylicBarFullScrenBase.Visibility == Visibility.Visible)
                {
                    var sbOut = new Storyboard();
                    var fadeOut = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromMilliseconds(160) };
                    Storyboard.SetTarget(fadeOut, AcrylicBarFullScrenBase);
                    Storyboard.SetTargetProperty(fadeOut, "Opacity");
                    sbOut.Completed += (s, e) => AcrylicBarFullScrenBase.Visibility = Visibility.Collapsed;
                    sbOut.Children.Add(fadeOut);
                    sbOut.Begin();
                }

                // Restaurar opacidade do header compacto e da search box
                if (HeaderCompactContent.Opacity < 1)
                {
                    var sb = new Storyboard();

                    var fadeHeader = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(200) };
                    Storyboard.SetTarget(fadeHeader, HeaderCompactContent);
                    Storyboard.SetTargetProperty(fadeHeader, "Opacity");

                    var fadeSearch = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(200) };
                    Storyboard.SetTarget(fadeSearch, SearchBox);
                    Storyboard.SetTargetProperty(fadeSearch, "Opacity");

                    sb.Children.Add(fadeHeader);
                    sb.Children.Add(fadeSearch);
                    sb.Begin();
                }
            }
        }

        private void Navigator_FullScreenModeChanged(object sender, bool isFullScreen)
        {
            if (!isFullScreen)
            {
                // Ao sair do fullscreen reseta tudo imediatamente
                var sysInfo = App.Services.GetRequiredService<ISystemInfoProvider>();
                if (!sysInfo.IsMobile() && !sysInfo.IsXbox())
                {
                    AcrylicBarFullScrenBase.Visibility = Visibility.Collapsed;
                    AcrylicBarFullScrenBase.Opacity = 0;
                }
                HeaderCompactContent.Opacity = 1;
                SearchBox.Opacity = 1;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() && Window.Current.Bounds.Width < 500)
            {
                VisualStateManager.GoToState(this, "CompactOverlayDesktopState", true);
            }

            if (!App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() && Window.Current.Bounds.Width >= 800 && Window.Current.Bounds.Width <= 1200)
            {
                VisualStateManager.GoToState(this, "SearchDesktopInlineState", false);
            }

            if (App.Services.GetRequiredService<ISystemInfoProvider>().IsMobile() && Window.Current.Bounds.Width >= 800)
            {
                VisualStateManager.GoToState(this, "SearchMobileInlineState", false);
            }

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

            //Se foi mandado pelo AutoggestBox da Sidebar
            if(sender.Name == "PaneSearchBox")
            {
                NavigationView.IsPaneOpen = false;
            }

            //Query
            string query = args.QueryText;
            sender.Text = string.Empty;

            // Remover o foco
            sender.IsEnabled = false;
            sender.IsEnabled = true;
            //Se query for valida executar busca
            if (!string.IsNullOrWhiteSpace(query))
            {
                ViewModel.InvokeSearchCommand.Execute(query);
            }
        }

        private void NavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            FocusCatcher.Focus(FocusState.Keyboard);
            InputPane.GetForCurrentView().TryHide();
            Debug.WriteLine("Abriu");
            PaneSearchBox.IsEnabled = true;
        }

        private void NavigationView_PaneOpened(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            Debug.WriteLine("Abriu");
            PaneSearchBox.IsEnabled = true;
        }
    }
}


// 🖼️ Imagem
/*
var image = new Image
{
    Width = 50,
    Height = 50,
    Margin = new Thickness(0, 0, 12, 0),
    Stretch = Stretch.UniformToFill,
    VerticalAlignment = VerticalAlignment.Top
};

image.SetBinding(Image.SourceProperty, new Binding
{
    Path = new PropertyPath("ImageUrl")
});
*/
