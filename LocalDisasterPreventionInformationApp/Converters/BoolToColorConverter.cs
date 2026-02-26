using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Converters {
    public class BoolToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // 選択時：薄いグレー、未選択：透明
            return (bool)value ? Color.FromArgb("#DDDDDD") : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}