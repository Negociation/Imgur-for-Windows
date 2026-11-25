using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Imgur.ViewModels.Settings
{
    public class SettingsViewModel : Observable
    {
        //D.I
        private ILocalSettings _localSettings;
        private INavigator _navigator;
        private ILiveTilesService _liveTilesService;
        private IRemindNotificationsService _remindNotificationsService;
        private IAccountNotificationsService _accountNotificationsService;
        private IDialogService _dialogService;
        private IAppLifeCycleService _appLifecycle;
        private readonly IUserContext _userContext;
        private readonly IAppLanguageService _appLanguage;
        private readonly IStartupLaunchService _startupLaunch;
        private readonly IStartTilePinService _startTilePin;

        private StartupLaunchState _startupState = StartupLaunchState.NotSupported;
        private bool _isSyncingStartupToggle;

        public SettingsViewModel(
            ILocalSettings localSettings,
            INavigator navigator,
            ILiveTilesService liveTilesService,
            IRemindNotificationsService remindNotificationsService,
            IAccountNotificationsService accountNotificationsService,
            IDialogService dialogService,
            IAppLifeCycleService appLifecycle,
            IUserContext userContext,
            IAppLanguageService appLanguage,
            IStartupLaunchService startupLaunch,
            IStartTilePinService startTilePin
            )
        {
            _localSettings = localSettings;
            _navigator = navigator;
            _liveTilesService = liveTilesService;
            _remindNotificationsService = remindNotificationsService;
            _accountNotificationsService = accountNotificationsService;
            _dialogService = dialogService;
            _appLifecycle = appLifecycle;
            _userContext = userContext;
            _appLanguage = appLanguage;
            _startupLaunch = startupLaunch;
            _startTilePin = startTilePin;

            ChangeLanguageCommand = new RelayCommand<object>(param =>
            {
                int index = -1;
                if (param is int i)
                    index = i;
                else if (param != null && int.TryParse(param.ToString(), out var parsed))
                    index = parsed;
                _ = ChangeLanguageAsync(index);
            });

            PinStartTileCommand = new RelayCommand(async () =>
            {
                await _startTilePin.PinAsync();
            });

            _ = RefreshStartupStateAsync();
        }

        public bool IsStartupLaunchSupported => _startupLaunch.IsSupported;

        public bool CanToggleStartupLaunch => StartupLaunchPresentation.CanToggle(_startupState);

        public bool StartupLaunchEnabled
        {
            get => StartupLaunchPresentation.IsOn(_startupState);
            set
            {
                if (_isSyncingStartupToggle || value == StartupLaunchPresentation.IsOn(_startupState))
                    return;
                _ = SetStartupLaunchAsync(value);
            }
        }

        public ICommand PinStartTileCommand { get; }


        public bool RemindNotificationsEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.RemindNotifications);
            set
            {
                _localSettings.Set(LocalSettingsConstants.RemindNotifications, value);
                _ = _remindNotificationsService.OnRemindNotificationsConfigChangedAsync();
            }
        }

        public bool AccountNotificationsEnabled
        {
            get => _localSettings.Get<bool>(LocalSettingsConstants.AccountNotifications);
            set
            {
                _localSettings.Set(LocalSettingsConstants.AccountNotifications, value);
                _ = _accountNotificationsService.OnAccountNotificationsConfigChangedAsync();
            }
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


        public string CustomAppId
        {
            get => _localSettings.Get<string>(LocalSettingsConstants.CustomClientId);
            set => _localSettings.Set(LocalSettingsConstants.CustomClientId, value);
        }

        public string CustomClientSecret
        {
            get => _localSettings.Get<string>(LocalSettingsConstants.CustomClientSecret);
            set => _localSettings.Set(LocalSettingsConstants.CustomClientSecret, value);
        }

        public AppLanguage SelectedLanguage
        {
            get
            {
                int raw = _localSettings.Get<int>(LocalSettingsConstants.SelectedLanguage);
                return AppLanguageInfo.FromStored(raw);
            }
        }

        public int SelectedLanguageIndex
        {
            get
            {
                AppLanguage selected = SelectedLanguage;
                IReadOnlyList<AppLanguage> all = AppLanguageInfo.All;
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] == selected)
                        return i;
                }
                return 0;
            }
        }

        public IReadOnlyList<string> LanguageOptions =>
            AppLanguageInfo.GetDisplayNames(_appLanguage.GetSystemDisplayLabel());

        public string SelectedLanguageOption
        {
            get
            {
                IReadOnlyList<string> options = LanguageOptions;
                int index = SelectedLanguageIndex;
                if (index >= 0 && index < options.Count)
                    return options[index];
                return options.Count > 0 ? options[0] : AppLanguageInfo.SystemDisplayName;
            }
        }

        public ICommand ChangeLanguageCommand { get; }

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

        private ICommand _openCustomApiKeyDialog;

        public ICommand OpenCustomApiKeyDialog
        {
            get
            {
                if (_openCustomApiKeyDialog == null)
                {
                    _openCustomApiKeyDialog = new RelayCommand(async () =>
                    {
                        try
                        {
                            await _dialogService.ShowCustomApiKeyDialog(this);
                        }
                        catch (Exception)
                        {
                        }
                        return;
                    });
                }
                return _openCustomApiKeyDialog;
            }
        }

        public ICommand RestartAppCommand => new RelayCommand(async () =>
        {
            await _userContext.LogoutAsync();
            await _appLifecycle.RestartToApplyCustomApiSettings();
        });

        private async System.Threading.Tasks.Task ChangeLanguageAsync(int index)
        {
            IReadOnlyList<AppLanguage> all = AppLanguageInfo.All;
            if (index < 0 || index >= all.Count)
                return;

            AppLanguage language = all[index];
            if (language == SelectedLanguage)
                return;

            try
            {
                await _appLanguage.ChangeLanguageAndRestartAsync(language);
            }
            catch (Exception)
            {
                OnPropertyChanged(nameof(SelectedLanguage));
                OnPropertyChanged(nameof(SelectedLanguageIndex));
                OnPropertyChanged(nameof(SelectedLanguageOption));
            }
        }

        private async System.Threading.Tasks.Task RefreshStartupStateAsync()
        {
            try
            {
                _startupState = await _startupLaunch.GetStateAsync();
            }
            catch (Exception)
            {
                _startupState = StartupLaunchState.NotSupported;
            }

            _isSyncingStartupToggle = true;
            OnPropertyChanged(nameof(IsStartupLaunchSupported));
            OnPropertyChanged(nameof(CanToggleStartupLaunch));
            OnPropertyChanged(nameof(StartupLaunchEnabled));
            _isSyncingStartupToggle = false;
        }

        private async System.Threading.Tasks.Task SetStartupLaunchAsync(bool enabled)
        {
            try
            {
                _startupState = await _startupLaunch.SetEnabledAsync(enabled);
            }
            catch (Exception)
            {
                // keep previous mapped state via refresh
            }

            await RefreshStartupStateAsync();
        }
    }
}
