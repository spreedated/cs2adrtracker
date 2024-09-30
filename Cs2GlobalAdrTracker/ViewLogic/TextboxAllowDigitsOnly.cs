using neXn.Lib.Wpf.ViewLogic;
using System;
using System.Globalization;

namespace Cs2GlobalAdrTracker.ViewLogic
{
    public class TextboxAllowDigitsOnly : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
            {
                return null;
            }

            string s = value as string;

            if (int.TryParse(s, out int d))
            {
                return d;
            }

            return s.Length > 0 ? s[..^1] : value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture);
        }
    }
}
