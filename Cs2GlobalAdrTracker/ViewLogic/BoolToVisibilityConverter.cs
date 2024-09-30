using neXn.Lib.Wpf.ViewLogic;
using System;
using System.Globalization;
using System.Windows;

namespace Cs2GlobalAdrTracker.ViewLogic
{
    internal class BoolToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
            {
                return value;
            }

            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility)
            {
                return value;
            }

            return (Visibility)value switch
            {
                Visibility.Visible => true,
                Visibility.Collapsed => false,
                Visibility.Hidden => false,
                _ => value,
            };
        }
    }
}
