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

            if (isFirstLaunch) {
                // 初回起動 → 会員登録ページ
                Preferences.Set("IsFirstLaunch", false);

                // 初回起動時のみ避難所データを追加
                Task.Run(async () => {
                    await _shelterService.FetchAndSaveShelterAsync();
                });

                return new Window(new RegisterPage());

            } else {
                // 2回目以降 → AppShell（トップページ）
                return new Window(new AppShell());
            }
        }
    }
}