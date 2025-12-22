using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Imgur.Uwp.Converters
{
    public class GallerySectionBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {

            switch ((int)value)
            {
                case 0: // Hot (Most Viral) - Verde menta/turquesa
                    return CreateGradientBrush(
                        Color.FromArgb(255, 102, 230, 184),  // #66e6b8 - verde claro
                        Color.FromArgb(255, 72, 220, 170),   // #48dcaa - verde-água
                        Color.FromArgb(255, 52, 210, 160),   // #34d2a0
                        Color.FromArgb(255, 32, 200, 150)    // #20c896 - verde escuro
                    );

                case 1: // User (User Sub) - Azul para roxo
                    return CreateGradientBrush(
                        Color.FromArgb(255, 88, 170, 255),   // #58aaff - azul claro
                        Color.FromArgb(255, 110, 140, 240),  // #6e8cf0 - azul-roxo
                        Color.FromArgb(255, 140, 110, 230),  // #8c6ee6 - roxo-azul
                        Color.FromArgb(255, 160, 90, 220)    // #a05adc - roxo
                    );

                case 2: // Top (Featured) - Laranja para rosa/vermelho
                    return CreateGradientBrush(
                        Color.FromArgb(255, 255, 180, 100),  // #ffb464 - laranja claro
                        Color.FromArgb(255, 255, 140, 110),  // #ff8c6e - laranja-coral
                        Color.FromArgb(255, 255, 100, 130),  // #ff6482 - rosa-coral
                        Color.FromArgb(255, 255, 70, 140)    // #ff468c - rosa-vermelho
                    );

                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        private LinearGradientBrush CreateGradientBrush(Color color1, Color color2, Color color3, Color color4)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 0)  // Gradiente horizontal
            };

            brush.GradientStops.Add(new GradientStop { Color = color1, Offset = 0 });
            brush.GradientStops.Add(new GradientStop { Color = color2, Offset = 0.33 });
            brush.GradientStops.Add(new GradientStop { Color = color3, Offset = 0.66 });
            brush.GradientStops.Add(new GradientStop { Color = color4, Offset = 1 });

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}