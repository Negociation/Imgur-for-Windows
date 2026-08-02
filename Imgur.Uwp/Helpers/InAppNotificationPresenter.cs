using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Imgur.Uwp.Helpers
{
    /// <summary>
    /// Mostra InApp notifications (TeachingTip) num <see cref="Panel"/> host.
    /// Usado pela Shell e pelas páginas fullscreen (vídeo/imagem).
    /// </summary>
    public sealed class InAppNotificationPresenter
    {
        private readonly Panel _host;
        private readonly IAppNotificationService _notifications;
        private bool _isAttached;

        public InAppNotificationPresenter(Panel host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _notifications = App.Services.GetRequiredService<IAppNotificationService>();
        }

        public void Attach()
        {
            if (_isAttached)
                return;

            _notifications.NotificationAdded += OnNotificationAdded;
            _isAttached = true;
        }

        public void Detach()
        {
            if (!_isAttached)
                return;

            _notifications.NotificationAdded -= OnNotificationAdded;
            _isAttached = false;
        }

        private async void OnNotificationAdded(object sender, NotificationViewModel notification)
        {
            if (notification == null || _host == null)
                return;

            await _host.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Show(notification));
        }

        public void Show(NotificationViewModel notification)
        {
            if (notification == null || _host == null)
                return;

            var notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20)
            };

            var teachingTip = new TeachingTip
            {
                Title = notification.Title ?? "New Message!",
                IsOpen = true,
                IsLightDismissEnabled = false,
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

            teachingTip.Closed += OnTeachingTipClosed;
            _host.Children.Add(teachingTip);
        }

        private void OnTeachingTipClosed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            sender.Closed -= OnTeachingTipClosed;
            _host.Children.Remove(sender);
        }

        private static UIElement CreateNotificationContent()
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

            var rootGrid = new Grid
            {
                MinWidth = 250
            };

            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var imageContainer = new Grid();
            Grid.SetColumn(imageContainer, 0);

            button.Loaded += (s, e) =>
            {
                var notification = button.DataContext as NotificationViewModel;
                bool isCircular = notification?.IsImageAvatarInfo ?? false;

                FrameworkElement imageElement;

                if (isCircular)
                {
                    var ellipse = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    var brush = new ImageBrush { Stretch = Stretch.UniformToFill };

                    if (!string.IsNullOrEmpty(notification?.ImageUrl))
                    {
                        brush.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(notification.ImageUrl));
                    }

                    ellipse.Fill = brush;
                    imageElement = ellipse;
                }
                else
                {
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

                    imageElement = image;
                }

                imageContainer.Children.Clear();
                imageContainer.Children.Add(imageElement);
            };

            rootGrid.Children.Add(imageContainer);

            var textGrid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -5, 0, 0),
            };

            textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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

            rootGrid.Children.Add(textGrid);

            button.Content = rootGrid;
            return button;
        }

        private static ICommand CreateCommandWithClose(ICommand originalCommand, TeachingTip teachingTip)
        {
            return new RelayCommand(() =>
            {
                if (originalCommand?.CanExecute(null) == true)
                    originalCommand.Execute(null);

                teachingTip.IsOpen = false;
            });
        }
    }
}
