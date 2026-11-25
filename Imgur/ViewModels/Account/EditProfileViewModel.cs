using Imgur.Api.Services.Contracts;
using Imgur.Api.Services.Models.Enum;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public class EditProfileViewModel : Observable
    {
        private const int BioMaxLength = 2500;

        private readonly INavigator _navigator;
        private readonly IUserContext _userContext;
        private readonly IDialogService _dialogService;
        private readonly IAppNotificationService _notifications;
        private readonly IAccountService _accountApi;

        private string _initialUsername = string.Empty;
        private string _initialBio = string.Empty;
        private string _initialAvatarApiValue = string.Empty;

        private string _username = string.Empty;
        private string _bio = string.Empty;
        private string _email = string.Empty;
        private string _avatarUrl = string.Empty;
        private string _coverUrl = string.Empty;
        private string _selectedAvatarApiValue = string.Empty;
        private ImgurAvatar? _selectedAvatar;

        public EditProfileViewModel(
            INavigator navigator,
            IUserContext userContext,
            IDialogService dialogService,
            IAppNotificationService notifications,
            IAccountService accountApi)
        {
            _navigator = navigator;
            _userContext = userContext;
            _dialogService = dialogService;
            _notifications = notifications;
            _accountApi = accountApi;
            SeedFromUser();
            _ = SeedFromApiAsync();
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username == value) return;
                _username = value ?? string.Empty;
                OnPropertyChanged(nameof(Username));
                NotifyChanges();
            }
        }

        public string Bio
        {
            get => _bio;
            set
            {
                if (_bio == value) return;
                _bio = value ?? string.Empty;
                OnPropertyChanged(nameof(Bio));
                OnPropertyChanged(nameof(BioCharCount));
                OnPropertyChanged(nameof(IsBioWithinLimit));
                NotifyChanges();
            }
        }

        /// <summary>Read-only from settings API — not part of HasChanges.</summary>
        public string Email
        {
            get => _email;
            private set
            {
                if (_email == value) return;
                _email = value ?? string.Empty;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            private set
            {
                if (_avatarUrl == value) return;
                _avatarUrl = value ?? string.Empty;
                OnPropertyChanged(nameof(AvatarUrl));
                OnPropertyChanged(nameof(HasAvatar));
            }
        }

        public string CoverUrl
        {
            get => _coverUrl;
            private set
            {
                if (_coverUrl == value) return;
                _coverUrl = value ?? string.Empty;
                OnPropertyChanged(nameof(CoverUrl));
                OnPropertyChanged(nameof(HasCover));
            }
        }

        public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarUrl);

        public bool HasCover => !string.IsNullOrWhiteSpace(CoverUrl);

        public string BioCharCount => (Bio?.Length ?? 0).ToString();

        public bool IsBioWithinLimit => (Bio?.Length ?? 0) <= BioMaxLength;

        public string SelectedAvatarApiValue => _selectedAvatarApiValue;

        public bool HasChanges =>
            !string.Equals(Username ?? string.Empty, _initialUsername) ||
            !string.Equals(Bio ?? string.Empty, _initialBio) ||
            !string.Equals(_selectedAvatarApiValue ?? string.Empty, _initialAvatarApiValue);

        public bool CanSave => HasChanges && IsBioWithinLimit;

        public double SaveButtonOpacity => CanSave ? 1.0 : 0.35;

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

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    // Stub — PUT settings wired in a later phase.
                    _saveCommand = new RelayCommand(() =>
                    {
                        if (!CanSave)
                            return;

                        _initialUsername = Username ?? string.Empty;
                        _initialBio = Bio ?? string.Empty;
                        _initialAvatarApiValue = _selectedAvatarApiValue ?? string.Empty;
                        NotifyChanges();
                    });
                }
                return _saveCommand;
            }
        }

        private ICommand _editAvatarCommand;
        public ICommand EditAvatarCommand
        {
            get
            {
                if (_editAvatarCommand == null)
                {
                    _editAvatarCommand = new RelayCommand(async () =>
                    {
                        var picked = await _dialogService.ShowAvatarPickerDialogAsync(_selectedAvatar);
                        if (!picked.HasValue)
                            return;

                        ApplyAvatarSelection(picked.Value);
                    });
                }
                return _editAvatarCommand;
            }
        }

        private ICommand _editCoverCommand;
        public ICommand EditCoverCommand
        {
            get
            {
                if (_editCoverCommand == null)
                    _editCoverCommand = new RelayCommand(NotifyFeatureUnavailable);
                return _editCoverCommand;
            }
        }

        private ICommand _deleteAccountCommand;
        public ICommand DeleteAccountCommand
        {
            get
            {
                if (_deleteAccountCommand == null)
                    _deleteAccountCommand = new RelayCommand(NotifyFeatureUnavailable);
                return _deleteAccountCommand;
            }
        }

        private ICommand _changePasswordCommand;
        public ICommand ChangePasswordCommand
        {
            get
            {
                if (_changePasswordCommand == null)
                    _changePasswordCommand = new RelayCommand(NotifyFeatureUnavailable);
                return _changePasswordCommand;
            }
        }

        private void ApplyAvatarSelection(ImgurAvatar avatar)
        {
            _selectedAvatar = avatar;
            _selectedAvatarApiValue = avatar.GetApiValue() ?? string.Empty;
            AvatarUrl = avatar.GetLocalImagePath() ?? string.Empty;
            NotifyChanges();
        }

        private void NotifyFeatureUnavailable()
        {
            _notifications.AddFeatureUnavailableNotification();
        }

        private void SeedFromUser()
        {
            var user = _userContext?.CurrentUser;
            _initialUsername = user?.Username ?? string.Empty;
            _initialBio = string.Empty;
            _username = _initialUsername;
            _bio = _initialBio;
            _email = string.Empty;
            _avatarUrl = user?.Avatar ?? string.Empty;
            _coverUrl = user?.Cover ?? string.Empty;
            _selectedAvatarApiValue = string.Empty;
            _initialAvatarApiValue = string.Empty;
            _selectedAvatar = null;
        }

        private async Task SeedFromApiAsync()
        {
            await SeedSettingsFromApiAsync();
            await SeedAvatarFromApiAsync();
        }

        private async Task SeedSettingsFromApiAsync()
        {
            try
            {
                var settings = await _accountApi.GetAccountSettingsAsync();
                if (settings != null && settings.Success && settings.Data != null)
                {
                    if (!string.IsNullOrWhiteSpace(settings.Data.account_url))
                    {
                        _initialUsername = settings.Data.account_url;
                        Username = settings.Data.account_url;
                    }

                    var bio = settings.Data.bio ?? string.Empty;
                    _initialBio = bio;
                    Bio = bio;
                    Email = settings.Data.email ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EditProfile] account settings seed failed: {ex}");
            }
        }

        private async Task SeedAvatarFromApiAsync()
        {
            try
            {
                var avatar = await _accountApi.GetAccountAvatarAsync();
                if (avatar == null || !avatar.Success || avatar.Data == null)
                    return;

                var seed = AvatarSeedResolver.Resolve(AvatarUrl, avatar.Data.avatar, avatar.Data.avatar_name);
                AvatarUrl = seed.DisplayUrl;

                // O preset só marca o item atual no seletor; não troca a foto exibida.
                if (seed.HasPreset)
                {
                    _selectedAvatar = seed.Preset;
                    _selectedAvatarApiValue = seed.PresetApiValue;
                    _initialAvatarApiValue = seed.PresetApiValue;
                    NotifyChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EditProfile] account avatar seed failed: {ex}");
            }
        }

        private void NotifyChanges()
        {
            OnPropertyChanged(nameof(HasChanges));
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(SaveButtonOpacity));
            OnPropertyChanged(nameof(SelectedAvatarApiValue));
        }
    }
}
