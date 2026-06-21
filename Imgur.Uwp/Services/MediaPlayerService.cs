using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {

        private readonly INavigator _navigator;
        private readonly IDispatcher _dispatcher;
        private readonly ILocalSettings _localSettings;
        private readonly SystemMediaTransportControls _stmc;
        private MediaPlayerElement _currentMediaPlayer;

        private int timeSpaces { get; set; }

        public MediaPlayerService(INavigator navigator, IDispatcher dispatcher, SystemMediaTransportControls systemMediaTransportControls, ILocalSettings localSettings)
        {
            this._navigator = navigator;
            this._dispatcher = dispatcher;
            this._navigator.NavigateInvoked += NavigateInvoked;
            this._localSettings = localSettings;
            this._stmc = systemMediaTransportControls;
        }


        private void NavigateInvoked(object sender, string e)
        {
            if (this._currentMediaPlayer != null)
            {
                this.StopMedia(null);
            }
        }

        public async Task<bool> PlayMedia(MediaPlayerElement mediaPlayer, Element element = null)
        {
            bool alreadyPlayed = mediaPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds > 0;

            if (_localSettings.Get<bool>(LocalSettingsConstants.DataNotifications) && IsUsingCellularData() && !alreadyPlayed)
            {
                var dialog = new MessageDialog(
                    "Fellow Imgiurian, you're about to play a video while using Mobile Data...",
                    "Play Media ?"
                );
                dialog.Commands.Add(new UICommand("Yes", null, "yes"));
                dialog.Commands.Add(new UICommand("No", null, "no"));
                dialog.DefaultCommandIndex = 1;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if (result.Id.ToString() == "no") return false;
            }

            if (this._currentMediaPlayer != null && this._currentMediaPlayer != mediaPlayer)
            {
                this._currentMediaPlayer.MediaPlayer.Pause();
                this._currentMediaPlayer.MediaPlayer.CommandManager.IsEnabled = false;
            }

            mediaPlayer.MediaPlayer.CommandManager.IsEnabled = true;
            this._currentMediaPlayer = mediaPlayer;

            // Reaplica os metadados DEPOIS do CommandManager estar ativo
            if (element != null)
            {
                var smtc = mediaPlayer.MediaPlayer.SystemMediaTransportControls;
                smtc.IsEnabled = true;
                smtc.IsPlayEnabled = true;
                smtc.IsPauseEnabled = true;

                var updater = smtc.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Title = string.IsNullOrEmpty(element.MediaTitle)
                    ? "Imgur"
                    : element.MediaTitle;
                updater.MusicProperties.Artist = element.MediaAuthor ?? string.Empty;
                updater.Update();
            }

            mediaPlayer.MediaPlayer.Play();
            return true;
        }

        private bool IsUsingCellularData()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (connectionProfile == null)
                return false;

            var networkAdapter = connectionProfile.NetworkAdapter;
            if (networkAdapter == null)
                return false;

            // Verifica se é conexão celular (WWAN)
            var ianaInterfaceType = networkAdapter.IanaInterfaceType;

            // 243 e 244 são os tipos IANA para WWAN (rede celular)
            return ianaInterfaceType == 243 || ianaInterfaceType == 244;
        }

        public void StopMedia(MediaPlayerElement mediaPlayer)
        {
            if (this._currentMediaPlayer != mediaPlayer)
            {
                this._currentMediaPlayer.MediaPlayer.CommandManager.IsEnabled = false;
                this._currentMediaPlayer.Source = null;
                this._currentMediaPlayer = null;
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
    }
}
