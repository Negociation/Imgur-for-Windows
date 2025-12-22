using Imgur.Api.Services.Models.Enum;
using Imgur.Models;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;


namespace Imgur.Uwp.Converters
{
    public class LoginInterceptorEnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();


            if (value != null && (value is Enum))
            {
                Debug.WriteLine(resourceLoader.GetString($"InterceptorHeader_{value}"));
                return resourceLoader.GetString($"InterceptorHeader_{value}");
            }
            Debug.WriteLine("rUIM");
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
