using System;
using System.Globalization;
using System.Windows.Data;

namespace MenuConfig2._0.Helpers
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && parameter != null && value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? int.Parse(parameter.ToString()) : Binding.DoNothing;
        }
    }
}


