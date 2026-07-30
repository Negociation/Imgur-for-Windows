using Imgur.Contracts;
using Imgur.Enums;
using Imgur.Models;
using Imgur.Services;
using Imgur.Uwp.Converters;
using Imgur.Uwp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using MediaModel = Imgur.Models.Media;

namespace Imgur.Uwp.Views.Media
{
    public sealed partial class FullScreenMediaView : Page
    {
        private FullScreenMediaPayload _payload;
        private readonly DispatcherTimer _positionTimer;
        private readonly DispatcherTimer _chromeTimer;
        private Storyboard _seekFeedbackStoryboard;
        private const int SeekSeconds = 10;
        private readonly TimestampToElapsedTimeConverter _timeConverter = new TimestampToElapsedTimeConverter();

        public FullScreenMediaView()
        {
            this.InitializeComponent();

            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _positionTimer.Tick += (s, e) =>
            {
                if (Player?.MediaPlayer?.PlaybackSession == null) return;
                PositionText.Text = Player.MediaPlayer.PlaybackSession.Position.ToString(@"mm\:ss");
                PlayerSlider.Value = Player.MediaPlayer.PlaybackSession.Position.TotalSeconds;
            };

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
            App.Services.GetRequiredService<IShareService>().Initialize();

            _payload = e.Parameter as FullScreenMediaPayload;
            if (_payload?.Element == null || string.IsNullOrEmpty(_payload.Element.Link))
                return;

            MuteBadge.Visibility = _payload.Element.IsSilentVideo ? Visibility.Visible : Visibility.Collapsed;
            BindPostInfo(_payload.Post);

            var source = MediaSource.CreateFromUri(new Uri(_payload.Element.Link));
            var item = new MediaPlaybackItem(source);
            Player.Source = item;
            Player.MediaPlayer.CommandManager.IsEnabled = true;
            Player.MediaPlayer.MediaOpened += OnMediaOpened;
            Player.MediaPlayer.PlaybackSession.PlaybackStateChanged += OnPlaybackStateChanged;
        }

        private void BindPostInfo(MediaModel post)
        {
            if (post == null)
            {
                DockTitle.Text = _payload?.Element?.MediaTitle ?? string.Empty;
                DockViewsRun.Text = "0";
                DockTimeRun.Text = string.Empty;
                return;
            }

            DockTitle.Text = post.Title ?? string.Empty;
            DockViewsRun.Text = post.Views.ToString();
            DockTimeRun.Text = _timeConverter.Convert(post.DateTime, typeof(string), null, null)?.ToString() ?? string.Empty;
            UpdateFavoriteMenuLabel(post.Favorite);
        }

        private void UpdateFavoriteMenuLabel(bool isFavorite)
        {
            FavoriteMenuItem.Text = isFavorite ? "Remove from Favourites" : "Add to Favourites";
        }

        private void OnMediaOpened(MediaPlayer sender, object args)
        {
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerSlider.Minimum = 0;
                PlayerSlider.Maximum = sender.PlaybackSession.NaturalDuration.TotalSeconds;
                sender.PlaybackSession.Position = _payload?.Position ?? TimeSpan.Zero;
                if (_payload?.WasPlaying == true)
                    sender.Play();
            });
        }

        private void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.Playing:
                        canPauseIcon.Visibility = Visibility.Visible;
                        canPlayIcon.Visibility = Visibility.Collapsed;
                        _positionTimer.Start();
                        break;
                    case MediaPlaybackState.Paused:
                        canPlayIcon.Visibility = Visibility.Visible;
                        canPauseIcon.Visibility = Visibility.Collapsed;
                        _positionTimer.Stop();
                        break;
                }
            });
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Player?.MediaPlayer != null)
            {
                Player.MediaPlayer.MediaOpened -= OnMediaOpened;
                Player.MediaPlayer.PlaybackSession.PlaybackStateChanged -= OnPlaybackStateChanged;
                Player.MediaPlayer.Pause();
            }

            _positionTimer.Stop();
            _chromeTimer.Stop();
            Player.Source = null;
            base.OnNavigatingFrom(e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Services.GetRequiredService<INavigator>().GoBack("fullscreenMedia");
        }

        private void Player_Tapped(object sender, TappedRoutedEventArgs e) => ShowChrome();

        private void Root_PointerEntered(object sender, PointerRoutedEventArgs e) => ShowChrome();

        private void Root_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _chromeTimer.Stop();
            _chromeTimer.Start();
        }

        private void ShowChrome()
        {
            // Mesmo fundo semi-transparente da bottom bar (acrylic)
            HeaderChrome.Background = PlayerTransportControls.Background;
            HeaderChrome.Opacity = PlayerTransportControls.Opacity;
            PlayerTransportControls.Visibility = Visibility.Visible;

            if (!string.IsNullOrWhiteSpace(DockTitle.Text) || !string.IsNullOrWhiteSpace(DockTimeRun.Text))
                TitleInfoPanel.Visibility = Visibility.Visible;

            _chromeTimer.Stop();
            _chromeTimer.Start();
        }

        private void HideChrome()
        {
            HeaderChrome.Background = new SolidColorBrush(Colors.Transparent);
            HeaderChrome.Opacity = 1;
            TitleInfoPanel.Visibility = Visibility.Collapsed;
            PlayerTransportControls.Visibility = Visibility.Collapsed;
        }

        private void TransportControlPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                Player.MediaPlayer.Pause();
            else
                Player.MediaPlayer.Play();
            ShowChrome();
        }

        private void PlayerSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Player?.MediaPlayer?.PlaybackSession == null) return;
            Player.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void Next_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Player.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(SeekSeconds);
            ShowSeekFeedback(forward: true);
            ShowChrome();
        }

        private void Back_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var pos = Player.MediaPlayer.PlaybackSession.Position - TimeSpan.FromSeconds(SeekSeconds);
            Player.MediaPlayer.PlaybackSession.Position = pos < TimeSpan.Zero ? TimeSpan.Zero : pos;
            ShowSeekFeedback(forward: false);
            ShowChrome();
        }

        private void ShowSeekFeedback(bool forward)
        {
            var target = forward ? SeekForwardFeedback : SeekBackwardFeedback;
            var other = forward ? SeekBackwardFeedback : SeekForwardFeedback;
            other.Opacity = 0;
            _seekFeedbackStoryboard?.Stop();

            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0 });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(120), Value = 1 });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(820), Value = 1 });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(1270), Value = 0 });
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            _seekFeedbackStoryboard = storyboard;
            storyboard.Begin();
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            var link = _payload?.Post?.Link ?? _payload?.Element?.Link;
            if (string.IsNullOrEmpty(link)) return;

            var clipboard = App.Services.GetRequiredService<IClipboardService>();
            if (await clipboard.GetTextAsync() == link)
                return;

            clipboard.SetText(link);
            App.Services.GetRequiredService<IAppNotificationService>().AddNotification(
                new NotificationViewModel { Message = "notification_clipboard_content" });
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            var title = _payload?.Post?.Title ?? _payload?.Element?.MediaTitle ?? "Imgur";
            var link = _payload?.Post?.Link ?? _payload?.Element?.Link;
            if (string.IsNullOrEmpty(link)) return;

            App.Services.GetRequiredService<IShareService>().ShareMediaPost(title, link);
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
            UpdateFavoriteMenuLabel(post.Favorite);
            _payload.FavoriteChanged?.Invoke();

            var actions = App.Services.GetRequiredService<UserMediaActionsService>();
            var result = await actions.FavoriteAsync(post.Id, post.IsAlbum);
            if (!result.IsSuccess)
            {
                post.Favorite = previous;
                UpdateFavoriteMenuLabel(post.Favorite);
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
    }
}
