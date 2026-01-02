using Imgur.Contracts;
using Imgur.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Imgur.Uwp.Controls
{
    public sealed partial class VideoViewElementControl : UserControl
    {
        private IMediaPlayerService _mediaPlayerService;
        private DispatcherTimer playerPosition;
        private DispatcherTimer playerTransportControlsVisibility;
        private readonly INavigator _navigator;

        public VideoViewElementControl()
        {
            this.InitializeComponent();

            //Get Service
            this._mediaPlayerService = App.Services.GetRequiredService<IMediaPlayerService>();
            this._mediaPlayerService.SetTimeSpaces(3); //Time for Forward and Backward

            //Fix for Creators Update to keep the AspectRatio
            this.SizeChanged += this.ResizeMediaControl;

            this.Player.MediaPlayer.MediaOpened += this.MediaLoadedRoutine;
            this.Player.MediaPlayer.MediaEnded += this.MediaEndedRoutine;
            this.Player.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChangedAsync;


            //Event Timers
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


        //Events Listeners
        private void OnIsFullWindowChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (Player.IsFullWindow)
            {
                // Entrou em fullscreen - mostrar overlay
                PlayerOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                // Saiu do fullscreen - esconder overlay
                PlayerOverlay.Visibility = Visibility.Collapsed;
                Player.AreTransportControlsEnabled = false;
            }
        }
            private async void MediaLoadedRoutine(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
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
            //Get media size
            int videoWidth = this.MediaElement. Width;
            int videoHeight = this.MediaElement.Height;

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

        //UI Events
        private void SetPlayingStatus()
        {
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



        //Dependencys
        public Element MediaElement
        {
            get { return (Element)GetValue(MediaElementProperty); }
            set { SetValue(MediaElementProperty, value); }
        }

        public static readonly DependencyProperty MediaElementProperty =
            DependencyProperty.Register("MediaElement", typeof(Element), typeof(VideoViewElementControl), new PropertyMetadata(0));



        private string Position
        {
            get { return (string)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }


        private static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(string), typeof(VideoViewElementControl), new PropertyMetadata(0));


        //Interaction Events
        private async void NextTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await _mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
            this._mediaPlayerService.GoForward();
        }

        private async void BackTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await _mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
            this._mediaPlayerService.GoBackward();
        }

        private void NeutralTapGridArea_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.GoToFullScreen();
        }

        private void Player_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.PlayerTransportControls.Visibility = Visibility.Visible;
            this.playerTransportControlsVisibility.Start();

        }


        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(await this._mediaPlayerService.PlayMedia(this.Player, this.MediaElement))
            {
                this.SetPlayingStatus();
                this.PlayerTransportControls.Visibility = Visibility.Visible;
                this.playerTransportControlsVisibility.Start();
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

               var success =  await this._mediaPlayerService.PlayMedia(this.Player, this.MediaElement);
               if (success)
               {
                    this.SetPlayingStatus();
               }
            }
        }


        private void PlayerSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.Player.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
        }


        //Timers 
        private void Timer_Tick(object sender, object e)
        {
            this.Position = this.Player.MediaPlayer.PlaybackSession.Position.ToString(@"mm\:ss");
            this.PlayerSlider.Value = this.Player.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        private void TransportControls_Timeout(object sender, object e)
        {
            this.PlayerTransportControls.Visibility = Visibility.Collapsed;
            this.playerTransportControlsVisibility.Stop();
        }

        private void GoToFullScreen()
        {
            this._navigator.NavigateMediaToFullScreen();
        }

        private void TransportControlFullScreen_Tapped(object sender, TappedRoutedEventArgs e)
        {

                Player.IsFullWindow = true;
                Player.AreTransportControlsEnabled = true;
            
        }
    }
}
