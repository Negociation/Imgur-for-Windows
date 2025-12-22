using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Imgur.Uwp.Converters{
    public class EqualBoolConverter : IValueConverter{
        #region IValueConverter Members


        public object Convert(object value, Type targetType, object parameter, string language)
        {

            //Debug.WriteLine((string)value + " -" + (string)parameter);

            if ((string)parameter != null){
                return string.Equals(((string)value), ((string)parameter));
            }        
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
