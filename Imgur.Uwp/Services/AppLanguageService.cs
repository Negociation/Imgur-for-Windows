using Imgur.Constants;
using Imgur.Contracts;
using Imgur.Helpers;
using Imgur.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Storage;

namespace Imgur.Uwp.Services
{
    /// <summary>
    /// WinRT language override + restart via <see cref="IAppLifeCycleService"/> (toast like API key).
    /// </summary>
    public sealed class AppLanguageService : IAppLanguageService
    {
        private readonly ILocalSettings _localSettings;
        private readonly IAppLifeCycleService _appLifecycle;

        public AppLanguageService(ILocalSettings localSettings, IAppLifeCycleService appLifecycle)
        {
            _localSettings = localSettings;
            _appLifecycle = appLifecycle;
        }

        public void ApplyFromSettings()
        {
            try
            {
                ApplyOverride(ReadSelectedLanguage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[AppLanguage] ApplyFromSettings: " + ex.Message);
            }
        }

        public string GetSystemDisplayLabel()
        {
            try
            {
                var label = ResourceLoader.GetForCurrentView().GetString("SettingsLanguageSystem");
                return string.IsNullOrWhiteSpace(label)
                    ? AppLanguageInfo.SystemDisplayName
                    : label;
            }
            catch
            {
                return AppLanguageInfo.SystemDisplayName;
            }
        }

        public async Task ChangeLanguageAndRestartAsync(AppLanguage language)
        {
            AppLanguage current = ReadSelectedLanguage();
            if (language == current)
                return;

            _localSettings.Set(LocalSettingsConstants.SelectedLanguage, (int)language);
            ApplyOverride(language);
            await _appLifecycle.RestartToApplyLanguageSettings();
        }

        /// <summary>
        /// Call before any UI/ResourceLoader so x:Uid picks the right language.
        /// </summary>
        public static void ApplyOverrideEarlyFromLocalSettings()
        {
            try
            {
                object raw = ApplicationData.Current.LocalSettings.Values[LocalSettingsConstants.SelectedLanguage];
                int value = raw is int i
                    ? i
                    : (int)AppLanguage.System;
                ApplyOverride(AppLanguageInfo.FromStored(value));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[AppLanguage] Early apply: " + ex.Message);
            }
        }

        /// <summary>
        /// System → empty override when OS has a shipped locale; else force en-US.
        /// Concrete languages set their tag.
        /// </summary>
        internal static void ApplyOverride(AppLanguage language)
        {
            string desired;
            if (AppLanguageInfo.IsSystem(language))
            {
                if (AppLanguageInfo.OsListContainsShipped(ApplicationLanguages.Languages))
                    desired = string.Empty;
                else
                    desired = AppLanguageInfo.GetTag(AppLanguage.English);
            }
            else
            {
                desired = AppLanguageInfo.GetTag(language);
            }

            string current = ApplicationLanguages.PrimaryLanguageOverride ?? string.Empty;
            if (!string.Equals(current, desired, StringComparison.OrdinalIgnoreCase))
                ApplicationLanguages.PrimaryLanguageOverride = desired;
        }

        private AppLanguage ReadSelectedLanguage()
        {
            try
            {
                int raw = _localSettings.Get<int>(LocalSettingsConstants.SelectedLanguage);
                return AppLanguageInfo.FromStored(raw);
            }
            catch
            {
                return AppLanguage.System;
            }
        }
    }
}
