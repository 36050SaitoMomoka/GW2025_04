public static class Responsive {
    public static double Scale {
        get {
            double width = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            if (width < 400) return 0.8;   // 小型スマホ
            if (width < 600) return 1.0;   // 標準スマホ
            if (width < 900) return 1.3;   // タブレット
            return 1.6;                    // PC
        }
    }
}