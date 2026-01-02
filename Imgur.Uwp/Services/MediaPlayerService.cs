using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Imgur.Uwp.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {

        private readonly SystemMediaTransportControls _systemMediaTransportControls;
        private readonly INavigator _navigator;
        private readonly IDispatcher _dispatcher;
        private readonly ILocalSettings _localSettings;
        private MediaPlayerElement _currentMediaPlayer;
        private SystemMediaTransportControls _transportControls;
        private SystemMediaTransportControls _currentTransportControls;


        private int timeSpaces { get; set; }

        public MediaPlayerService(INavigator navigator, IDispatcher dispatcher, SystemMediaTransportControls systemMediaTransportControls, ILocalSettings localSettings)
        {
            this._systemMediaTransportControls = systemMediaTransportControls;
            this._navigator = navigator;
            this._dispatcher = dispatcher;
            this._navigator.NavigateInvoked += NavigateInvoked;
            this._localSettings = localSettings;


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

        public async Task<bool> PlayMedia(MediaPlayerElement mediaPlayer, Element element = null)
        {
            bool alreadyPlayed = mediaPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds > 0;

            // Só pergunta se está em dados móveis E o vídeo nunca foi reproduzido
            if (_localSettings.Get<bool>(LocalSettingsConstants.DataNotifications) && IsUsingCellularData() && !alreadyPlayed)
            {
                // Mostra dialog de confirmação
                var dialog = new MessageDialog(
                    "Fellow Imgiurian, you're about to play a video while using Mobile Data...",
                    "Play Media ?"
                );

                dialog.Commands.Add(new UICommand("Yes", null, "yes"));
                dialog.Commands.Add(new UICommand("No", null, "no"));
                dialog.DefaultCommandIndex = 1; // "Não" como padrão
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsync();

                // Se escolheu "Não", cancela
                if (result.Id.ToString() == "no")
                {
                    return false;
                }
            }

            // Continua normalmente se não for dados móveis ou se confirmou
            if (this._currentMediaPlayer != null && this._currentMediaPlayer != mediaPlayer)
            {
                this._currentMediaPlayer.MediaPlayer.Pause();
                this._systemMediaTransportControls.DisplayUpdater.ClearAll();
                this._systemMediaTransportControls.IsEnabled = false;
            }

            mediaPlayer.MediaPlayer.Play();
            this._currentMediaPlayer = mediaPlayer;
            this.UpdateSystemMediaTransportControls(mediaPlayer, element);

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
