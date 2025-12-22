using Imgur.Api.Services.Models.Enum;
using Imgur.Models;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;


namespace Imgur.Uwp.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();


            if (value != null && (value is Enum))
            {
                switch (value)
                {
                    case GallerySection section:
                        return resourceLoader.GetString($"category_{section}");
                    case GallerySort sort:
                        return resourceLoader.GetString($"sort_{sort}");
                    case GalleryWindow sort:
                        return resourceLoader.GetString($"sort_{sort}");
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
