using Imgur.Constants;
using Imgur.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Imgur.Services
{
    public class ImgurApiCredentialsProvider : IImgurApiCredentialsProvider
    {
        private readonly ILocalSettings _localSettings;

        public ImgurApiCredentialsProvider(ILocalSettings localSettings)
        {
            _localSettings = localSettings;
        }

        public string ClientId
        {
            get
            {
                _localSettings.Set<string>(LocalSettingsConstants.ClientId, "b6c4abc4061d423");
                var custom = _localSettings.Get<string>(LocalSettingsConstants.CustomClientId);
                if (string.IsNullOrWhiteSpace(custom))
                    return _localSettings.Get<string>(LocalSettingsConstants.ClientId);
                return custom;
            }
        }

        public string ClientSecret
        {
            get
            {
                var custom = _localSettings.Get<string>(LocalSettingsConstants.CustomClientSecret);
                if (string.IsNullOrWhiteSpace(custom))
                    return _localSettings.Get<string>(LocalSettingsConstants.ClientSecret);
                return custom;
            }
        }
    }
}
