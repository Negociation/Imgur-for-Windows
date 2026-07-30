using Imgur.Contracts;
using Imgur.Models;
using Imgur.Uwp.Models;
using Imgur.Uwp.Views.Media;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Imgur.Uwp.Controls
{
    public sealed partial class VideoViewElementControl : UserControl
    {
        /// <summary>
        /// Feature flag POC: true = FullScreenMediaView via RootFrame; false = IsFullWindow atual.
        /// </summary>
        private const bool newMode = true;

        private IMediaPlayerService _mediaPlayerService;
        private DispatcherTimer playerPosition;
        private DispatcherTimer playerTransportControlsVisibility;
        private readonly INavigator _navigator;
        private readonly ISystemInfoProvider _systemInfo;
        private Storyboard _seekFeedbackStoryboard;

        public VideoViewElementControl()
        {
            this.InitializeComponent();

            this.Player.MediaPlayer.CommandManager.IsEnabled = false;

            this._mediaPlayerService = App.Services.GetRequiredService<IMediaPlayerService>();
            this._mediaPlayerService.SetTimeSpaces(10);
            this._systemInfo = App.Services.GetRequiredService<ISystemInfoProvider>();

            this.SizeChanged += this.ResizeMediaControl;

            this.Player.MediaPlayer.MediaOpened += this.MediaLoadedRoutine;
            this.Player.MediaPlayer.MediaEnded += this.MediaEndedRoutine;
            this.Player.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;

            this.playerPosition = new DispatcherTimer();
            this.playerPosition.Interval = TimeSpan.FromMilliseconds(1000);
            this.playerPosition.Tick += Timer_Tick;
            this.playerTransportControlsVisibility = new DispatcherTimer();
            this.playerTransportControlsVisibility.Interval = TimeSpan.FromSeconds(5);
            this.playerTransportControlsVisibility.Tick += TransportControls_Timeout;

            Player.RegisterPropertyChangedCallback(
                MediaPlayerElement.IsFullWindowProperty,
                OnIsFullWindowChanged);

            this._navigator = App.Services.GetRequiredService<INavigator>();
        }

        private void OnIsFullWindowChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (Player.IsFullWindow)
            {
                PlayerOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                PlayerOverlay.Visibility = Visibility.Collapsed;
                Player.AreTransportControlsEnabled = false;
            }
        }

        private async void MediaLoadedRoutine(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.PlayerSlider.Minimum = 0;
                this.PlayerSlider.Maximum = this.Player.MediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                this.SetStopStatus();
            });
        }

        private async void MediaEndedRoutine(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.SetStopStatus());
        }

        private async void PlaybackSession_PlaybackStateChangedAsync(MediaPlaybackSession sender, object args)
        {
            switch (sender.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.SetPlayingStatus());
                    break;
                case MediaPlaybackState.Buffering:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.SetBufferingStatus());
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.SetPausedStatus());
                    break;
            }
        }

        private void ResizeMediaControl(object sender, SizeChangedEventArgs e)
        {
            if (IsPlayerHostedExternally || MediaElement == null)
                return;

            int videoWidth = this.MediaElement.Width;
            int videoHeight = this.MediaElement.Height;
            if (videoWidth <= 0 || videoHeight <= 0)
                return;

            if (this.ActualWidth < 500)
            {
                this.Player.Width = this.ActualWidth;
                this.Player.Height = this.Player.Width * videoHeight / videoWidth;
            }
            else
            {
                this.Player.Height = this.Player.Height < 500 ? this.ActualHeight : 400;
                this.Player.Width = videoWidth * this.Player.Height / videoHeight;
            }
        }

        private void SetPlayingStatus()
        {
            UpdatePositionText();
            this.playerPosition.Start();
            this.PlayerOverlay.Visibility = Visibility.Collapsed;
            this.PlayerInteractionCommandsOverlay.Visibility = Visibility.Visible;
            this.canPauseIcon.Visibility = Visibility.Visible;
            this.canPlayIcon.Visibility = Visibility.Collapsed;
        }

        private void SetPausedStatus()
        {
            this.playerPosition.Stop();
            this.canPlayIcon.Visibility = Visibility.Visible;
            this.canPauseIcon.Visibility = Visibility.Collapsed;
        }

        private void SetBufferingStatus()
        {
        }

        private void SetStopStatus()
        {
            if (this.Player.MediaPlayer.PlaybackSession.Position.TotalSeconds == this.Player.MediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds)
            {
                this.PlayerOverlay.Visibility = Visibility.Visible;
                this.PlayerInteractionCommandsOverlay.Visibility = Visibility.Collapsed;
                this.playerPosition.Stop();
            }
        }

        public Element MediaElement
        {
            get { return (Element)GetValue(MediaElementProperty); }
            set { SetValue(MediaElementProperty, value); }
        }

        public static readonly DependencyProperty MediaElementProperty =
            DependencyProperty.Register("MediaElement", typeof(Element), typeof(VideoViewElementControl), new PropertyMetadata(null, OnMediaElementChanged));

        private static void OnMediaElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoViewElementControl control && e.NewValue is Element element)
            {
                control.BuildPlaybackSource(element);
                control.MuteBadge.Visibility = element.IsSilentVideo ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BuildPlaybackSource(Element element)
        {
            if (string.IsNullOrEmpty(element?.Link)) return;

            var source = MediaSource.CreateFromUri(new Uri(element.Link));
            var item = new MediaPlaybackItem(source);

            this.Player.Source = item;

            var props = item.GetDisplayProperties();
            props.Type = MediaPlaybackType.Music;
            props.MusicProperties.Title = string.IsNullOrEmpty(element.MediaTitle) ? "Imgur" : element.MediaTitle;
            props.MusicProperties.Artist = element.MediaAuthor ?? string.Empty;

            item.ApplyDisplayProperties(props);
        }

        private string Position
        {
            get { return (string)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        private static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(string), typeof(VideoViewElementControl), new PropertyMetadata("00:00"));

        private async void NextTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await _mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
            this._mediaPlayerService.GoForward();
            ShowSeekFeedback(forward: true);
        }

        private async void BackTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await _mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
            this._mediaPlayerService.GoBackward();
            ShowSeekFeedback(forward: false);
        }

        private void NeutralTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            RequestUpvoteOverlay();
        }

        private void Player_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowTransportControls();
        }

        private void RootMedia_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_systemInfo.IsMobile())
                return;

            if (Player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing
                || Player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                ShowTransportControls();
            }
        }

        private void RootMedia_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_systemInfo.IsMobile())
                return;

            // Reinicia o timeout para esconder após sair do hover
            this.playerTransportControlsVisibility.Stop();
            this.playerTransportControlsVisibility.Start();
        }

        private void ShowTransportControls()
        {
            this.PlayerTransportControls.Visibility = Visibility.Visible;
            this.playerTransportControlsVisibility.Stop();
            this.playerTransportControlsVisibility.Start();
        }

        private void ShowSeekFeedback(bool forward)
        {
            var target = forward ? SeekForwardFeedback : SeekBackwardFeedback;
            var other = forward ? SeekBackwardFeedback : SeekForwardFeedback;
            other.Opacity = 0;

            _seekFeedbackStoryboard?.Stop();

            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)),
                Value = 0
            });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)),
                Value = 1
            });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(820)),
                Value = 1
            });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1270)),
                Value = 0
            });

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);

            _seekFeedbackStoryboard = storyboard;
            storyboard.Begin();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (await this._mediaPlayerService.PlayMedia(this.Player, this.MediaElement))
            {
                this.SetPlayingStatus();
                ShowTransportControls();
                FindParentMediaView()?.NotifyMainPlayerPlaybackStarted();
            }
        }

        private async void TransportControlPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.Player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                this.Player.MediaPlayer.Pause();
                this.SetPausedStatus();
            }
            else
            {
                var success = await this._mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
                if (success)
                {
                    this.SetPlayingStatus();
                    FindParentMediaView()?.NotifyMainPlayerPlaybackStarted();
                }
            }
        }

        private void PlayerSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.Player.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdatePositionText();
        }

        private void UpdatePositionText()
        {
            try
            {
                if (Player?.MediaPlayer?.PlaybackSession == null)
                {
                    this.Position = "00:00";
                    return;
                }

                var position = Player.MediaPlayer.PlaybackSession.Position;
                this.Position = position.ToString(@"mm\:ss");
                this.PlayerSlider.Value = position.TotalSeconds;
            }
            catch
            {
                this.Position = "00:00";
            }
        }

        private void TransportControls_Timeout(object sender, object e)
        {
            this.PlayerTransportControls.Visibility = Visibility.Collapsed;
            this.playerTransportControlsVisibility.Stop();
        }

        private void GoToFullScreen()
        {
            if (newMode)
            {
                var session = Player.MediaPlayer.PlaybackSession;
                var wasPlaying = session.PlaybackState == MediaPlaybackState.Playing;
                if (wasPlaying)
                    Player.MediaPlayer.Pause();

                var mediaVm = FindParentMediaView()?.DataContext as MediaViewModel;
                _navigator.NavigateMediaToFullScreen(new FullScreenMediaPayload
                {
                    Element = MediaElement,
                    Post = mediaVm?.CurrentMedia,
                    Position = session.Position,
                    WasPlaying = wasPlaying,
                    FavoriteChanged = () => mediaVm?.NotifyFavoriteChanged()
                });
            }
            else
            {
                Player.IsFullWindow = true;
                Player.AreTransportControlsEnabled = true;
            }
        }

        private void TransportControlFullScreen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GoToFullScreen();
        }

        public bool IsActivelyPlaying
        {
            get
            {
                try
                {
                    return Player?.MediaPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsPlayerHostedExternally { get; private set; }

        private int _playerChildIndex = 3;
        private double _savedPlayerWidth;
        private double _savedPlayerHeight;

        /// <summary>
        /// Reparents the MediaPlayerElement into an external host (in-app compact) without touching SetMediaPlayer.
        /// Leaves a same-size placeholder so list scroll layout does not jump.
        /// </summary>
        public bool MovePlayerToHost(Panel host)
        {
            if (host == null || Player == null || IsPlayerHostedExternally)
                return false;

            try
            {
                _savedPlayerWidth = Player.ActualWidth > 0 ? Player.ActualWidth : Player.Width;
                _savedPlayerHeight = Player.ActualHeight > 0 ? Player.ActualHeight : Player.Height;
                if (_savedPlayerWidth <= 0 || double.IsNaN(_savedPlayerWidth))
                    _savedPlayerWidth = ActualWidth > 0 ? ActualWidth : 320;
                if (_savedPlayerHeight <= 0 || double.IsNaN(_savedPlayerHeight))
                    _savedPlayerHeight = 180;

                PlayerPlaceholder.Width = _savedPlayerWidth;
                PlayerPlaceholder.Height = _savedPlayerHeight;
                PlayerPlaceholder.Visibility = Visibility.Visible;
                MinHeight = _savedPlayerHeight;

                if (Player.Parent is Panel parent)
                {
                    _playerChildIndex = parent.Children.IndexOf(Player);
                    parent.Children.Remove(Player);
                }

                Player.Width = double.NaN;
                Player.Height = double.NaN;
                Player.HorizontalAlignment = HorizontalAlignment.Stretch;
                Player.VerticalAlignment = VerticalAlignment.Stretch;
                host.Children.Insert(0, Player);

                PlayerInteractionCommandsOverlay.Visibility = Visibility.Collapsed;
                PlayerTransportControls.Visibility = Visibility.Collapsed;
                IsPlayerHostedExternally = true;
                return true;
            }
            catch (Exception)
            {
                PlayerPlaceholder.Visibility = Visibility.Collapsed;
                MinHeight = 0;
                return false;
            }
        }

        public bool RestorePlayerFromHost()
        {
            if (!IsPlayerHostedExternally || Player == null)
                return false;

            try
            {
                if (Player.Parent is Panel host)
                    host.Children.Remove(Player);

                var index = Math.Max(0, Math.Min(_playerChildIndex, RootMedia.Children.Count));
                RootMedia.Children.Insert(index, Player);

                if (_savedPlayerWidth > 0 && !double.IsNaN(_savedPlayerWidth))
                    Player.Width = _savedPlayerWidth;
                if (_savedPlayerHeight > 0 && !double.IsNaN(_savedPlayerHeight))
                    Player.Height = _savedPlayerHeight;

                PlayerPlaceholder.Visibility = Visibility.Collapsed;
                PlayerPlaceholder.Width = double.NaN;
                PlayerPlaceholder.Height = double.NaN;
                MinHeight = 0;
                IsPlayerHostedExternally = false;
                ResizeMediaControl(this, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void RequestUpvoteOverlay()
        {
            FindParentMediaView()?.ShowUpvoteFeedback();
        }

        private MediaView FindParentMediaView()
        {
            DependencyObject current = this;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is MediaView mediaView)
                    return mediaView;
            }
            return null;
        }
    }
}
