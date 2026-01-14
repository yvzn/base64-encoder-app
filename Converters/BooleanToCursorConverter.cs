using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Base64Utils.Converters
{
    public class BooleanToCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool canExpand && canExpand)
            {
                return Cursors.Hand;
            }
            return Cursors.Arrow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
