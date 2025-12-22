using Imgur.Constants;
using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Imgur.Uwp.Services
{
    public class LocalSettings : ILocalSettings
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
