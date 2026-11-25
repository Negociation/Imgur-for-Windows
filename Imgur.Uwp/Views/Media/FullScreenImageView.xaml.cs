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
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MediaModel = Imgur.Models.Media;

namespace Imgur.Uwp.Views.Media
{
    public sealed partial class FullScreenImageView : Page
    {
        private const double MinZoom = 1.0;
        private const double MaxZoom = 5.0;
        private const double WheelZoomStep = 1.12;

        private FullScreenMediaPayload _payload;
        private readonly DispatcherTimer _chromeTimer;
        private InAppNotificationPresenter _notificationPresenter;
        private readonly TimestampToElapsedTimeConverter _timeConverter = new TimestampToElapsedTimeConverter();
        private static readonly SolidColorBrush FavoriteGreen =
            new SolidColorBrush(Color.FromArgb(255, 0x1b, 0xb7, 0x6e));
        private static readonly SolidColorBrush FavoriteWhite =
            new SolidColorBrush(Colors.White);

        private int _naturalWidth;
        private int _naturalHeight;
        private double _displayWidth;
        private double _displayHeight;
        private double _zoom = MinZoom;

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

            SizeChanged += (s, e) => UpdateImageLayout();
            ImageArea.SizeChanged += (s, e) => UpdateImageLayout();
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

            _naturalWidth = 0;
            _naturalHeight = 0;
            FullImage.Source = CompatibleImageLoader.CreateBitmap(_payload.Element.Link);
            ResetZoom();
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
            if (_zoom > 1.01)
                return;
            ShowChrome();
            e.Handled = true;
        }

        private void TopTapZone_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_zoom > 1.01)
                return;
            ShowChrome();
        }

        private void ShowChrome()
        {
            if (_zoom > 1.01)
                return;

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

        private void FullImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            CaptureNaturalSize();
            UpdateImageLayout();
            ResetZoom();
        }

        private void ImageArea_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (ImageTransform == null)
                return;

            bool scaled = Math.Abs(e.Delta.Scale - 1.0) > 0.001;
            if (scaled)
            {
                var origin = e.Position;
                ZoomAt(ImageTransform.ScaleX * e.Delta.Scale, origin.X, origin.Y);
            }

            // Pan so com zoom (ou durante pinch em andamento com escala > 1)
            if (_zoom > 1.01 || scaled)
            {
                ImageTransform.TranslateX += e.Delta.Translation.X;
                ImageTransform.TranslateY += e.Delta.Translation.Y;
                ClampTranslation();
            }

            UpdatePanOverlays();
            e.Handled = true;
        }

        private void ImageArea_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ClampTranslation();
            UpdatePanOverlays();
        }

        private void ImageArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var pos = e.GetPosition(ImageArea);
            if (_zoom > 1.05)
                ZoomAt(MinZoom, pos.X, pos.Y);
            else
                ZoomAt(2.5, pos.X, pos.Y);
            e.Handled = true;
        }

        private void ImageArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(ImageArea);
            if (point.Properties.IsHorizontalMouseWheel)
                return;

            int delta = point.Properties.MouseWheelDelta;
            if (delta == 0)
                return;

            double factor = delta > 0 ? WheelZoomStep : 1.0 / WheelZoomStep;
            ZoomAt(_zoom * factor, point.Position.X, point.Position.Y);
            e.Handled = true;
        }

        private void ZoomAt(double targetZoom, double viewportX, double viewportY)
        {
            targetZoom = Clamp(targetZoom, MinZoom, MaxZoom);
            if (Math.Abs(targetZoom - _zoom) < 0.001)
            {
                if (targetZoom <= MinZoom + 0.01)
                    ResetZoom();
                return;
            }

            // Ponto no conteúdo antes do zoom
            double contentX = (viewportX - ImageTransform.TranslateX) / _zoom;
            double contentY = (viewportY - ImageTransform.TranslateY) / _zoom;

            _zoom = targetZoom;
            ImageTransform.ScaleX = _zoom;
            ImageTransform.ScaleY = _zoom;

            // Mantém o ponto sob o dedo/cursor
            ImageTransform.TranslateX = viewportX - contentX * _zoom;
            ImageTransform.TranslateY = viewportY - contentY * _zoom;

            if (_zoom <= MinZoom + 0.01)
            {
                ResetZoom();
                return;
            }

            ClampTranslation();
            UpdatePanOverlays();
        }

        private void ClampTranslation()
        {
            if (ImageArea == null || _displayWidth <= 0 || _displayHeight <= 0)
                return;

            double vw = ImageArea.ActualWidth;
            double vh = ImageArea.ActualHeight;
            if (vw <= 0 || vh <= 0)
                return;

            double scaledW = _displayWidth * _zoom;
            double scaledH = _displayHeight * _zoom;

            // Host preenche a área; escala em torno do centro → limite = metade do overflow.
            if (scaledW <= vw)
                ImageTransform.TranslateX = 0;
            else
            {
                double maxX = (scaledW - vw) / 2.0;
                ImageTransform.TranslateX = Clamp(ImageTransform.TranslateX, -maxX, maxX);
            }

            if (scaledH <= vh)
                ImageTransform.TranslateY = 0;
            else
            {
                double maxY = (scaledH - vh) / 2.0;
                ImageTransform.TranslateY = Clamp(ImageTransform.TranslateY, -maxY, maxY);
            }
        }

        private void UpdatePanOverlays()
        {
            bool canPan = _zoom > 1.01;
            if (TopTapZone != null)
                TopTapZone.IsHitTestVisible = !canPan;

            if (canPan)
            {
                HeaderChrome.HorizontalAlignment = HorizontalAlignment.Left;
                HeaderChrome.Background = new SolidColorBrush(Colors.Transparent);
                TitleInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetZoom()
        {
            _zoom = MinZoom;
            if (ImageTransform != null)
            {
                ImageTransform.ScaleX = MinZoom;
                ImageTransform.ScaleY = MinZoom;
                ImageTransform.TranslateX = 0;
                ImageTransform.TranslateY = 0;
            }
            UpdatePanOverlays();
        }

        private void CaptureNaturalSize()
        {
            var bmp = FullImage.Source as BitmapImage;
            if (bmp == null)
                return;

            if (bmp.PixelWidth > 0 && bmp.PixelHeight > 0)
            {
                _naturalWidth = bmp.PixelWidth;
                _naturalHeight = bmp.PixelHeight;
            }
        }

        private void UpdateImageLayout()
        {
            if (FullImage == null || ImageArea == null)
                return;

            double viewportW = ImageArea.ActualWidth;
            double viewportH = ImageArea.ActualHeight;
            if (viewportW <= 0 || viewportH <= 0)
                return;

            CaptureNaturalSize();
            double naturalW = _naturalWidth;
            double naturalH = _naturalHeight;
            if (naturalW <= 0 || naturalH <= 0)
                return;

            double scale = Math.Min(1.0, Math.Min(viewportW / naturalW, viewportH / naturalH));
            _displayWidth = naturalW * scale;
            _displayHeight = naturalH * scale;

            FullImage.Width = _displayWidth;
            FullImage.Height = _displayHeight;

            ClampTranslation();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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
