using DatabaseLayer.Models;
using neXn.Lib.Wpf.ViewLogic;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace Cs2GlobalAdrTracker.ViewLogic
{
    internal class StringToOutcomeConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AdrRecord.Outcomes)
            {
                return null;
            }

            AdrRecord.Outcomes o = (AdrRecord.Outcomes)value;

            if (o == AdrRecord.Outcomes.Unkown)
            {
                return "None";
            }

            return o.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((ComboBoxItem)value).Content is not string || ((string)((ComboBoxItem)value).Content).Length <= 0)
            {
                return AdrRecord.Outcomes.Unkown;
            }

            switch ((string)((ComboBoxItem)value).Content)
            {
                case "Win":
                    return AdrRecord.Outcomes.Win;
                case "Lose":
                    return AdrRecord.Outcomes.Lose;
                case "Draw":
                    return AdrRecord.Outcomes.Draw;
                default:
                    return AdrRecord.Outcomes.Unkown;
            }
        }
    }
}
