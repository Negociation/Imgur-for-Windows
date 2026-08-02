using Imgur.Api.Services.Contracts;
using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Models;
using Imgur.Services;
using Imgur.Uwp.Converters;
using Imgur.Uwp.Helpers;
using Imgur.Uwp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MediaModel = Imgur.Models.Media;

namespace Imgur.Uwp.Views.Media
{
    public sealed partial class FullScreenImageView : Page
    {
        private FullScreenMediaPayload _payload;
        private readonly DispatcherTimer _chromeTimer;
        private InAppNotificationPresenter _notificationPresenter;
        private readonly TimestampToElapsedTimeConverter _timeConverter = new TimestampToElapsedTimeConverter();
        private static readonly SolidColorBrush FavoriteGreen =
            new SolidColorBrush(Color.FromArgb(255, 0x1b, 0xb7, 0x6e));
        private static readonly SolidColorBrush FavoriteWhite =
            new SolidColorBrush(Colors.White);

        public FullScreenImageView()
        {
            this.InitializeComponent();
            _notificationPresenter = new InAppNotificationPresenter(Root);
            _chromeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _chromeTimer.Tick += (s, e) =>
            {
                HideChrome();
                _chromeTimer.Stop();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _notificationPresenter?.Attach();
            SetupPcTitleBar();

            _payload = e.Parameter as FullScreenMediaPayload;
            if (_payload?.Element == null || string.IsNullOrEmpty(_payload.Element.Link))
                return;

            BindPostInfo(_payload.Post, _payload.AuthorUsername);
            UpdateFavoriteIcon(_payload.Post?.Favorite == true);

            FullImage.Source = CompatibleImageLoader.CreateBitmap(_payload.Element.Link);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _notificationPresenter?.Detach();
            _chromeTimer.Stop();
            FullImage.Source = null;
            base.OnNavigatingFrom(e);
        }

        private void SetupPcTitleBar()
        {
            var sys = App.Services.GetRequiredService<ISystemInfoProvider>();
            var isPc = !sys.IsMobile() && !sys.IsXbox();
            if (!isPc)
            {
                PcTitleBar.Visibility = Visibility.Collapsed;
                PcTitleBarRow.Height = new GridLength(0);
                return;
            }

            PcTitleBar.Visibility = Visibility.Visible;
            PcTitleBarRow.Height = new GridLength(32);
            Window.Current.SetTitleBar(PcTitleBar);
        }

        private void BindPostInfo(MediaModel post, string authorUsername)
        {
            if (post == null)
            {
                DockTitle.Text = _payload?.Element?.MediaTitle ?? string.Empty;
                DockAuthorRun.Text = authorUsername ?? string.Empty;
                DockTimeRun.Text = string.Empty;
                DockMetaSep.Text = string.Empty;
                return;
            }

            DockTitle.Text = post.Title ?? _payload?.Element?.MediaTitle ?? string.Empty;

            var author = authorUsername ?? string.Empty;
            var time = _timeConverter.Convert(post.DateTime, typeof(string), null, null)?.ToString() ?? string.Empty;

            DockAuthorRun.Text = author;
            DockTimeRun.Text = time;
            DockMetaSep.Text = !string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(time) ? " • " : string.Empty;
        }

        private void UpdateFavoriteIcon(bool isFavorite)
        {
            FavoriteIcon.Foreground = isFavorite ? FavoriteGreen : FavoriteWhite;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Services.GetRequiredService<INavigator>().GoBack("fullscreenImage");
        }

        private void TopTapZone_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowChrome();
            e.Handled = true;
        }

        private void TopTapZone_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ShowChrome();
        }

        private void ShowChrome()
        {
            // Fundo semi-transparente atrás do título (header alinha à esquerda)
            HeaderChrome.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0));
            HeaderChrome.HorizontalAlignment = HorizontalAlignment.Stretch;

            if (!string.IsNullOrWhiteSpace(DockTitle.Text)
                || !string.IsNullOrWhiteSpace(DockAuthorRun.Text)
                || !string.IsNullOrWhiteSpace(DockTimeRun.Text))
            {
                TitleInfoPanel.Visibility = Visibility.Visible;
            }

            _chromeTimer.Stop();
            _chromeTimer.Start();
        }

        private void HideChrome()
        {
            HeaderChrome.Background = new SolidColorBrush(Colors.Transparent);
            HeaderChrome.HorizontalAlignment = HorizontalAlignment.Left;
            TitleInfoPanel.Visibility = Visibility.Collapsed;
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        {
            var post = _payload?.Post;
            if (post == null || string.IsNullOrEmpty(post.Id)) return;

            var userContext = App.Services.GetRequiredService<IUserContext>();
            if (!userContext.IsAuthenticated)
            {
                await App.Services.GetRequiredService<IDialogService>()
                    .ShowLoginInterceptorDialog(LoginInterceptorEnum.Like);
                return;
            }

            var previous = post.Favorite;
            post.Favorite = !post.Favorite;
            UpdateFavoriteIcon(post.Favorite);
            _payload.FavoriteChanged?.Invoke();

            var actions = App.Services.GetRequiredService<UserMediaActionsService>();
            var result = await actions.FavoriteAsync(post.Id, post.IsAlbum);
            if (!result.IsSuccess)
            {
                post.Favorite = previous;
                UpdateFavoriteIcon(post.Favorite);
                _payload.FavoriteChanged?.Invoke();
                App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                    new NotificationViewModel { Message = "notification_favourite_error_content" });
            }
            else if (post.Favorite)
            {
                App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                    new NotificationViewModel { Message = "notification_favourite_success_content" });
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            var url = _payload?.Element?.Link;
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var folderDialog = App.Services.GetRequiredService<IFolderDialogService>();
                var folderToken = await folderDialog.PickFolderAsync();
                if (folderToken == null) return;

                var uri = new Uri(url);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"download_{DateTime.Now:yyyyMMdd_HHmmss}";

                var storage = App.Services.GetRequiredService<IContentStorageService>();
                await storage.SaveAsync(uri, folderToken, fileName);

                App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                    new NotificationViewModel { Message = "notification_download_success_content" });
            }
            catch (Exception)
            {
                App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                    new NotificationViewModel { Message = "notification_download_error_content" });
            }
        }

        private async void CopyLink_Click(object sender, RoutedEventArgs e)
        {
            var link = _payload?.Element?.Link;
            if (string.IsNullOrEmpty(link)) return;

            var clipboard = App.Services.GetRequiredService<IClipboardService>();
            if (await clipboard.GetTextAsync() == link)
                return;

            clipboard.SetText(link);
            App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                new NotificationViewModel { Message = "notification_clipboard_content" });
        }
    }
}
