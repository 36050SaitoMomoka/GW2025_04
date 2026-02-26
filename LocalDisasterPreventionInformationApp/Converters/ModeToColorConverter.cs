using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Converters {
    public class ModeToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string current = value as string;      // 現在のモード
            string mode = parameter as string;     // ボタン側のモード

            if (current == mode)
                return Color.FromArgb("#8CAFA4");  // 選択中の色（濃い）

            return Color.FromArgb("#DDDDDD");      // 非選択の色（薄い）
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}