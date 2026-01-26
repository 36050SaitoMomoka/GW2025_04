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

            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);        // 初回起動かどうか

            // 初回起動時
            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);

                //初回起動時にのみDBにデータ追加
                Task.Run(async () => {
                    await _shelterService.FetchAndSaveShelterAsync();
                });

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