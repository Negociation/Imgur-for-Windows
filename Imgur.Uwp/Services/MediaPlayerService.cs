using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Diagnostics;
using Windows.Media;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {

        private readonly SystemMediaTransportControls _systemMediaTransportControls;
        private readonly INavigator _navigator;
        private readonly IDispatcher _dispatcher;
        private MediaPlayerElement _currentMediaPlayer;
        private SystemMediaTransportControls _transportControls;
        private SystemMediaTransportControls _currentTransportControls;


        private int timeSpaces { get; set; }

        public MediaPlayerService(INavigator navigator, IDispatcher dispatcher, SystemMediaTransportControls systemMediaTransportControls)
        {
            this._systemMediaTransportControls = systemMediaTransportControls;
            this._navigator = navigator;
            this._dispatcher = dispatcher;
            this._navigator.NavigateInvoked += NavigateInvoked;


            // Obtém o MediaTransportControls do MediaPlayerElement
            //_mediaTransportControls = _currentMediaPlayer.TransportControls;

        }


        private void NavigateInvoked(object sender, string e)
        {
            if (this._currentMediaPlayer != null)
            {
                this.StopMedia(null);
            }
        }

        public void PlayMedia(MediaPlayerElement mediaPlayer, Element element = null)
        {
            if (this._currentMediaPlayer != null && this._currentMediaPlayer != mediaPlayer)
            {
                this._currentMediaPlayer.MediaPlayer.Pause();

                // Remove a conexão do STC com o player anterior
                this._systemMediaTransportControls.DisplayUpdater.ClearAll();
                this._systemMediaTransportControls.IsEnabled = false;


            }
            mediaPlayer.MediaPlayer.Play();
            this._currentMediaPlayer = mediaPlayer;
            this.UpdateSystemMediaTransportControls(mediaPlayer, element);
        }



        public void StopMedia(MediaPlayerElement mediaPlayer)
        {
            if (this._currentMediaPlayer != mediaPlayer)
            {
                this._currentMediaPlayer.Source = null;
                this._currentMediaPlayer = null;
            }
        }

        private void UpdateSystemMediaTransportControls(MediaPlayerElement mediaPlayer, Element element = null)
        {

            if (this._currentMediaPlayer != null)
            {
                this._systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                this._systemMediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;
                this._systemMediaTransportControls.IsNextEnabled = this._systemMediaTransportControls.IsPreviousEnabled = this._systemMediaTransportControls.IsPlayEnabled = this._systemMediaTransportControls.IsPauseEnabled = true;
                if (element != null)
                {
                    this._systemMediaTransportControls.DisplayUpdater.MusicProperties.Title = element.MediaTitle;
                    this._systemMediaTransportControls.DisplayUpdater.MusicProperties.Artist = element.MediaAuthor;
                    //_systemMediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Thumbnail.png"));
                }
                else
                {
                    this._systemMediaTransportControls.DisplayUpdater.MusicProperties.Title = "Imgur";
                    this._systemMediaTransportControls.DisplayUpdater.MusicProperties.Artist = "Playing Video";
                }
                this._systemMediaTransportControls.DisplayUpdater.Update();
            }
            else
            {
                this._systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                this._systemMediaTransportControls.DisplayUpdater.Update();
            }
        }

        public TimeSpan GetPosition()
        {
            if (_currentMediaPlayer != null)
            {
                return _currentMediaPlayer.MediaPlayer.PlaybackSession.Position;
            }
            return TimeSpan.Zero;
        }

        public void GoForward()
        {
            if (this.timeSpaces > 0)
            {
                this._currentMediaPlayer.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(this.timeSpaces);
            }
        }

        public void GoBackward()
        {
            if (this.timeSpaces > 0)
            {
                this._currentMediaPlayer.MediaPlayer.PlaybackSession.Position -= TimeSpan.FromSeconds(this.timeSpaces);
            }
        }

        public void SetTimeSpaces(int timespace)
        {
            this.timeSpaces = timespace;
        }

        private async void SystemMediaTransportControls_PlayerFunctions(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            Debug.WriteLine("TODO");
        }
    }
}
