using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Imgur.Uwp.Converters
{
    public class TimestampToElapsedTimeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {

            // converte o valor para um objeto DateTime
            var timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((long)value);
            var elapsed = DateTime.UtcNow - timestamp;


            var resourceLoader = ResourceLoader.GetForCurrentView();

            // retorna a string que representa o tempo decorrido
            if (elapsed.TotalSeconds < 60)
            {
                return String.Format(resourceLoader.GetString("SecondsElapsedFormat"), (long)elapsed.TotalSeconds);
            }
            else if (elapsed.TotalMinutes < 60)
            {
                return String.Format(resourceLoader.GetString("MinutesElapsedFormat"), (long)elapsed.TotalMinutes);
            }
            else if (elapsed.TotalHours < 24)
            {
                return String.Format(resourceLoader.GetString("HoursElapsedFormat"), (long)elapsed.TotalHours);
            }
            else
            {
                return String.Format(resourceLoader.GetString("DaysElapsedFormat"), (long)elapsed.TotalDays);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
    #endregion
}
