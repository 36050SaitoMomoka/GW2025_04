using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {
        public App() {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

            //初回起動
            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);

                var shell = new AppShell();
                shell.GoToAsync("//RegisterPage");
                return new Window(shell);
            } else {
                return new Window(new AppShell());
            }
        }
    }
}