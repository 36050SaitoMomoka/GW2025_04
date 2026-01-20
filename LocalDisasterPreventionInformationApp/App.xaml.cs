using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;
using LocalDisasterPreventionInformationApp.Services;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {

        private readonly ShelterService _shelterService;

        public App(ShelterService shelterService) {
            InitializeComponent();
            _shelterService = shelterService;
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            //bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);
            bool isFirstLaunch = true;

            //初回起動
            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);
                return new Window(new RegisterPage());
            } else {
                return new Window(new AppShell());
            }
        }
    }
}