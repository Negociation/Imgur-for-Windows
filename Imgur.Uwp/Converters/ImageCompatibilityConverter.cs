using Imgur.Uwp.Helpers;
using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Imgur.Uwp.Converters
{
    /// <summary>
    /// Converte URL → BitmapImage. WebP via CompatibleImageLoader (BitmapDecoder do OS).
    /// Uso futuro: Image.Source="{Binding Url, Converter={StaticResource ImageCompatibilityConverter}, ConverterParameter=400}"
    /// </summary>
    public class ImageCompatibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;
            if (string.IsNullOrWhiteSpace(url))
                return null;

            int? maxWidth = null;
            if (parameter != null && int.TryParse(parameter.ToString(), out var w) && w > 0)
                maxWidth = w;

            return CompatibleImageLoader.CreateBitmap(url, maxWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
