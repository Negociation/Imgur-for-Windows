using Imgur.Contracts;
using Imgur.Models;
using Imgur.Uwp.Views.Explorer;
using Imgur.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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

        public ShellView()
        {
            this.LoadDesignAdapters();
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShellViewModel>();
            Loaded += Page_Loaded;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            

            //Retrieve new UI Notifications
            if (e.PropertyName == nameof(ViewModel.NotificationStack))
            {
                this.AddUINotification(ViewModel.NotificationStack[ViewModel.NotificationStack.Count - 1]);
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
                Debug.WriteLine("deveria mudar");
                VisualStateManager.GoToState(this, "SearchMobileInlineState", false);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.Frame = MainFrame;
            MainFrame.Navigate(typeof(ExplorerView));
           

            ViewModel.Initialize();
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

        private void AddUINotification(Notification notification)
        {

            DispatcherTimer notificationTimer = new DispatcherTimer();
            notificationTimer.Interval = TimeSpan.FromSeconds(10);
            TeachingTip _notificationTeachingTip = new TeachingTip();
            _notificationTeachingTip.Title = "New Notification!";
            _notificationTeachingTip.Subtitle = notification.Message;
            _notificationTeachingTip.IsOpen = true;
            _notificationTeachingTip.PreferredPlacement = TeachingTipPlacementMode.BottomRight;
            _notificationTeachingTip.PlacementMargin = new Thickness(0, 0, -20, 40);
            _notificationTeachingTip.Background = new SolidColorBrush(Color.FromArgb(255, 100, 50, 249));
            _notificationTeachingTip.Closed += RemoveUINotification;
            this.ShellGrid.Children.Add(_notificationTeachingTip);

            notificationTimer.Tick += (sender, e) =>
            {
                _notificationTeachingTip.IsOpen = false;
                notificationTimer.Stop();

            };

            notificationTimer.Start();

        }

        private void RemoveUINotification(Microsoft.UI.Xaml.Controls.TeachingTip sender, Microsoft.UI.Xaml.Controls.TeachingTipClosedEventArgs args)
        {
            ShellGrid.Children.Remove(sender);
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

            //Se query for valida executar busca
            if (!string.IsNullOrWhiteSpace(query))
            {
                ViewModel.InvokeSearchCommand.Execute(query);
            }
        }


        private void NavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            //FocusCatcher.Focus(FocusState.Keyboard);
            InputPane.GetForCurrentView().TryHide();
        }
    }
}
