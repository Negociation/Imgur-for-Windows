using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Imgur.Uwp.Converters
{

    public class DecodeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;
            if (string.IsNullOrWhiteSpace(url)) return null;

            int width = 400;
            if (parameter != null && int.TryParse(parameter.ToString(), out var p)) width = p;

            var bmp = new BitmapImage
            {
                DecodePixelType = DecodePixelType.Logical,
                DecodePixelWidth = width
            };
            bmp.UriSource = new Uri(url);
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}