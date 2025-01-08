using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace LiveCaptionsTranslator.converters
{
    public class IsLastItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[1] is not IList list)
                return false;

            return list.Count > 0 && list[^1] == values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 