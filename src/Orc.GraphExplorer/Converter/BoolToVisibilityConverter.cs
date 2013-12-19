using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Orc.GraphExplorer.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility retVal = Visibility.Collapsed;
            bool locVal = (bool)value;
            if (locVal)
                retVal = Visibility.Visible;
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility locVal = (Visibility)value;
            bool retVal = false;
            if (locVal == Visibility.Visible)
                retVal = true;
            return retVal;
        }
    }
}
