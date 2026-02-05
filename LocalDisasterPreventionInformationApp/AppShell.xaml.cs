using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Services;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace LocalDisasterPreventionInformationApp {
    public partial class AppShell : Shell {

        private readonly ShelterService _shelterService;
        private readonly AppDatabase _db;
        private bool _initialized = false;

        public AppShell(ShelterService shelterService, AppDatabase db) {
            InitializeComponent();
            _shelterService = shelterService;
            _db = db;

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
        }
    }
}