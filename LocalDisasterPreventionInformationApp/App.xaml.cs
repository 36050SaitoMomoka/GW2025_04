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
            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

            //初回起動
            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);

                //初回起動時にのみDBにデータ追加
                Task.Run(async () => {
                    await _shelterService.FetchAndSaveShelterAsync();
                });

                var shell = new AppShell();
                shell.GoToAsync("//RegisterPage");
                return new Window(shell);
            } else {
                return new Window(new AppShell());
            }
        }
    }
}