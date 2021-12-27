﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Services{
    public interface ILocalSettings{


        /// <summary>
        /// Raised when a settings is set.
        /// </summary>
        event EventHandler<string> SettingSet;

        /// <summary>
        /// Saves settings into persistent local
        /// storage.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <param name="value">The value to save.</param>
        void Set<T>(string settingKey, T value);

        /// <summary>
        /// Retrieves the value for the desired settings key.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <returns>The desired value or returns the default value.</returns>
        T Get<T>(string settingKey);

    }
}
