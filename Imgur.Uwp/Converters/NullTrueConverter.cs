using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imgur.Uwp.Converters
{
    public class NullTrueConverter{
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language){
        
            return (value is null) ? true : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
