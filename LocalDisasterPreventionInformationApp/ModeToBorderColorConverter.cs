using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Converters {
    public class ModeToBorderColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string currentMode = value as string;
            string thisMode = parameter as string;

            if (currentMode == thisMode)
                return Color.FromArgb("#B2BEB5");

            return Colors.Transparent; // 非選択時は枠なし
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}