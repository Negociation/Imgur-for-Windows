using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Imgur.Uwp.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var param = parameter?.ToString() ?? "#1bb76e|#FFFFFF";
            var parts = param.Split('|');

            var trueColor = parts.Length > 0 ? parts[0] : "#1bb76e";
            var falseColor = parts.Length > 1 ? parts[1] : "#FFFFFF";

            bool isTrue = value is bool b && b;

            return new SolidColorBrush(isTrue
                ? HexToColor(trueColor)
                : HexToColor(falseColor));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();

        private Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte a = 255;
            byte r, g, b;

            if (hex.Length == 6)
            {
                r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            }
            else if (hex.Length == 8)
            {
                a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                b = System.Convert.ToByte(hex.Substring(6, 2), 16);
            }
            else
            {
                return Colors.White;
            }

            return Color.FromArgb(a, r, g, b);
        }
    }
    #endregion
}
