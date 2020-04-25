using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoClicker
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value.Equals(parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value.Equals(true) ? parameter : Binding.DoNothing;
    }
}
