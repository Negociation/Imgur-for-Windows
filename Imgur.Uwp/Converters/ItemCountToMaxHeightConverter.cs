using System;
using Windows.UI.Xaml.Data;
using System.Collections;

namespace Imgur.Uwp.Converters
{
    public class ItemCountToMaxHeightConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return double.PositiveInfinity;

            int count = 0;

            // Se for uma coleção, pega o count
            if (value is ICollection collection)
            {
                count = collection.Count;
            }
            else if (value is int intValue)
            {
                count = intValue;
            }

            // Se tiver apenas 1 item, limita a 2000
            // Senão, sem limite (infinito)
            return count == 1 ? 2000.0 : double.PositiveInfinity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}