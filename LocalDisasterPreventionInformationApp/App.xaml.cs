using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {
        public App() {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

            if (isFirstLaunch) {
                // 初回起動 → 会員登録ページ
                Preferences.Set("IsFirstLaunch", false);
                return new Window(new RegisterPage());
            } else {
                // 2回目以降 → AppShell（トップページ）
                return new Window(new AppShell());
            }
        }
    }
}