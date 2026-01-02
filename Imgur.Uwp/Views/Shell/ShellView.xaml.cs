using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using Imgur.Uwp.Converters;
using Imgur.Uwp.Views.Explorer;
using Imgur.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            

            //Retrieve new UI Notifications
            if (e.PropertyName == nameof(ViewModel.NotificationStack))
            {
                this.AddUINotification(ViewModel.NotificationStack[ViewModel.NotificationStack.Count - 1]);
            }

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

        private void AddUINotification(NotificationViewModel notification)
        {
            var notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };

            var teachingTip = new TeachingTip
            {
                Title = notification.Title ?? "New Message!",
                IsOpen = true,
                PreferredPlacement = TeachingTipPlacementMode.BottomRight,
                PlacementMargin = new Thickness(0, 0, -20, 40),
                Background = new SolidColorBrush(Color.FromArgb(255, 100, 50, 249)),
            };


            if (notification.ImageUrl != null)
            {
                notification.ActionCommand = CreateCommandWithClose(notification.ActionCommand, teachingTip);
                teachingTip.DataContext = notification;
                teachingTip.Content = CreateNotificationContent();

            }
            else
            {
                teachingTip.Subtitle = notification.Message;
                notificationTimer.Tick += (s, e) =>
                {
                    teachingTip.IsOpen = false;
                    notificationTimer.Stop();
                };

                notificationTimer.Start();
            }

            teachingTip.Closed += RemoveUINotification;
            ShellGrid.Children.Add(teachingTip);

          
        }
        private UIElement CreateNotificationContent()
        {
            var button = new Button
            {
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0)
            };

            button.SetBinding(Button.CommandProperty, new Binding
            {
                Path = new PropertyPath("ActionCommand")
            });

            // 🔹 Grid principal
            var rootGrid = new Grid
            {
                MinWidth = 250
            };

            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 🖼️ Imagem
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

            Grid.SetColumn(image, 0);

            // 🔹 Grid de texto (controle vertical real)
            var textGrid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -5, 0, 0),

            };

            textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 🔸 Subtitle / Author
            var subtitleText = new TextBlock
            {
                FontSize = 12,
                Opacity = 0.7,
                Margin = new Thickness(0, 0, 0, 2),
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxLines = 1
            };

            subtitleText.SetBinding(TextBlock.TextProperty, new Binding
            {
                Path = new PropertyPath("Extra")
            });

            Grid.SetRow(subtitleText, 1);

            // 🔸 Message / Title
            var messageText = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, -2, 0, 0),
                MaxLines = 2
            };

            messageText.SetBinding(TextBlock.TextProperty, new Binding
            {
                Path = new PropertyPath("Message")
            });

            Grid.SetRow(messageText, 0);

            textGrid.Children.Add(subtitleText);
            textGrid.Children.Add(messageText);

            Grid.SetColumn(textGrid, 1);

            rootGrid.Children.Add(image);
            rootGrid.Children.Add(textGrid);

            button.Content = rootGrid;
            return button;
        }

        private void RemoveUINotification(Microsoft.UI.Xaml.Controls.TeachingTip sender, Microsoft.UI.Xaml.Controls.TeachingTipClosedEventArgs args)
        {
            ShellGrid.Children.Remove(sender);
        }

        private ICommand CreateCommandWithClose(
            ICommand originalCommand,
            TeachingTip teachingTip)
        {
            return new RelayCommand(() =>
            {
                if (originalCommand?.CanExecute(null) == true)
                    originalCommand.Execute(null);

                teachingTip.IsOpen = false;
            });
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
            //FocusCatcher.Focus(FocusState.Keyboard);
            InputPane.GetForCurrentView().TryHide();
        }
    }
}
