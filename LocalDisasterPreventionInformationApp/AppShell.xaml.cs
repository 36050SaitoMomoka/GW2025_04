using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Services;
using System.Diagnostics;

namespace LocalDisasterPreventionInformationApp {
    public partial class AppShell : Shell {

        private readonly ShelterService _shelterService;

        public AppShell(ShelterService shelterService) {
            InitializeComponent();
            _shelterService = shelterService;

            //ヘッダー・フッター紐づけ
            BindingContext = new AppShellViewModel();

            //ページ遷移用
            Routing.RegisterRoute("chat", typeof(Pages.Friends.ChatPage));                  // チャット

            Routing.RegisterRoute("safety", typeof(Pages.Friends.SafetyListPage));          // 安否ボタン一覧

            Routing.RegisterRoute("product", typeof(Pages.Stock.ProductRegisterPage));      // 商品登録

            Routing.RegisterRoute("language", typeof(Pages.Setting.LanguagePage));          // 多言語対応

            Routing.RegisterRoute("setting", typeof(Pages.Setting.FontPage));               //フォント選択

            Routing.RegisterRoute("mypage", typeof(Pages.Setting.MyPage));                  // マイページ

        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

            if (isFirstLaunch) {
                Preferences.Set("IsFirstLaunch", false);

                // 初回起動時だけ避難所データを読み込む
                await _shelterService.FetchAndSaveShelterAsync();

                // RegisterPage へ遷移
                await GoToAsync("//RegisterPage");
            }
            //} else {
            //    // 2回目以降
            //    await GoToAsync("//TopPage");
            //}
        }
    }
}