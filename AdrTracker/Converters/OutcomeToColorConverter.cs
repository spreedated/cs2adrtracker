using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using DatabaseLayer.Models;
using System;
using System.Globalization;

namespace AdrTracker.Converters
{
    public class OutcomeToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AdrRecord.Outcomes v)
            {
                switch (v)
                {
                    case AdrRecord.Outcomes.Unset:
                        return new ImmutableSolidColorBrush(Color.Parse("#DDDDDD"));
                    case AdrRecord.Outcomes.Loss:
                        return new ImmutableSolidColorBrush(Color.Parse("#B22222"));
                    case AdrRecord.Outcomes.Win:
                        return new ImmutableSolidColorBrush(Color.Parse("#228B22"));
                    case AdrRecord.Outcomes.Draw:
                        return new ImmutableSolidColorBrush(Color.Parse("#FFD700"));
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
