using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WPF.BespokeControls.Converter
{
    /// <summary>
    /// Converts the inverse of a <see cref="bool"/> to an appropriate <see cref="Visibility"/>.
    /// </summary>
    public class InverseBoolToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                boolValue = !boolValue;
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue)
            {
                return visibilityValue != Visibility.Visible;
            }

            return true;
        }
    }
}
