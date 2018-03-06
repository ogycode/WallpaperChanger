using System;
using System.Globalization;
using System.Windows.Data;

namespace Wallpapers.Client.Core
{
    public class SelectedIndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value == -1 ? false : true;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? 0 : -1;
    }
}
