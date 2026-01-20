using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {
        public App() {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);        // 初回起動かどうか

            // 初回起動時
            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);

                var shell = new AppShell();
                shell.GoToAsync("//RegisterPage");      // 会員登録ページへ遷移
                return new Window(shell);
            } else {
                // 通常起動
                return new Window(new AppShell());
            }
        }
    }
}