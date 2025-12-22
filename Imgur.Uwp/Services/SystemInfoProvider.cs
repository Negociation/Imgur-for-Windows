using Imgur.Contracts;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Imgur.Uwp.Services
{
    public class SystemInfoProvider : ISystemInfoProvider
    {


        public bool IsMinBuild(int version)
        {
            switch (version)
            {
                case 15063:
                    return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4);
                case 16299:
                    return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);
                case 17134:
                    return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6);
                case 17763:
                    return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
                case 18362:
                    return ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8);
                default:
                    return false;
            }
        }

        public bool IsContinuum() => ((AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") && (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)) ? true : false;

        public bool IsMobile() => (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? true : false);

        public bool IsXbox() => (AnalyticsInfo.VersionInfo.DeviceFamily == "Xbox" ? true : false);

        public bool IsFirstRun() => SystemInformation.IsFirstRun;

    }
}
