using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Converters {
    public class ModeToBorderColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string current = value as string;
            string mode = parameter as string;

            if (current == mode)
                return Colors.Black;   // 選択中は黒枠

            return Colors.Transparent; // 非選択は枠なし
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}