using System;
using System.Diagnostics;
using System.IO;
using Windows.Media.Core;
using Windows.UI.Xaml.Data;


namespace Imgur.Uwp.Converters
{
    public class StringToURIConverter : IValueConverter{
        #region IValueConverter Members


        public object Convert(object value, Type targetType, object parameter, string language){  
            return MediaSource.CreateFromUri(new Uri((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
