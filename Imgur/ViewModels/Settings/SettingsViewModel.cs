using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Helpers;
using System.Windows.Input;

namespace Imgur.ViewModels.Settings
{
    public class SettingsViewModel : Observable
    {
        //D.I
        private ILocalSettings _localSettings;
        private INavigator _navigator;
        private ILiveTilesService _liveTilesService;

        public SettingsViewModel(ILocalSettings localSettings, INavigator navigator, ILiveTilesService liveTilesService)
        {
            _localSettings = localSettings;
            _navigator = navigator;
            _liveTilesService = liveTilesService;

        }


        public bool RemindNotificationsEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications);
            set => _localSettings.Set(LocalSettingsConstants.RemindNotifications, value);
        }

        public bool DataNotificationsEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.DataNotifications);
            set => _localSettings.Set(LocalSettingsConstants.DataNotifications, value);
        }

        public bool HDonWifiEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.HDonWifi);
            set => _localSettings.Set(LocalSettingsConstants.HDonWifi, value);
        }

        public bool LiveTilesEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.LiveTiles);
            set
            {
                _localSettings.Set(LocalSettingsConstants.LiveTiles, value);
                _ = _liveTilesService.OnLiveTilesConfigChangedAsync();
            }
        }

        public bool AutoPlayEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.AutoPlay);
            set => _localSettings.Set(LocalSettingsConstants.AutoPlay, value);
        }

        public int ThumbnailsSize
        {
            get => _localSettings.Get<int>(LocalSettingsConstants.ThumbSize);
            set => _localSettings.Set(LocalSettingsConstants.ThumbSize, value);
        }

        private ICommand _leaveCurrentPage;

        public ICommand LeaveCurrentPage
        {
            get
            {
                if (_leaveCurrentPage == null)
                {
                    _leaveCurrentPage = new RelayCommand(() =>
                    {
                        _navigator.GoBack();
                    });
                }
                return _leaveCurrentPage;
            }
        }
    }
}
