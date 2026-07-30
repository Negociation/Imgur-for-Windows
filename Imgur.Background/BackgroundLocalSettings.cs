using Imgur.Constants;
using Imgur.Contracts;
using System;
using Windows.Storage;

namespace Imgur.Background
{
    /// <summary>
    /// ILocalSettings mínimo para a Background Task (sem evento UI).
    /// </summary>
    internal sealed class BackgroundLocalSettings : ILocalSettings
    {
        public event EventHandler<string> SettingSet;

        public T Get<T>(string settingKey)
        {
            object result = ApplicationData.Current.LocalSettings.Values[settingKey];
            return result is null ? (T)LocalSettingsConstants.Defaults[settingKey] : (T)result;
        }

        public void Set<T>(string settingKey, T value)
        {
            ApplicationData.Current.LocalSettings.Values[settingKey] = value;
            SettingSet?.Invoke(this, settingKey);
        }
    }
}
