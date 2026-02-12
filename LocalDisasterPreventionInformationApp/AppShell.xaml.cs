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
            Routing.RegisterRoute("product", typeof(Pages.Stock.ProductRegisterPage));          // 商品登録

            Routing.RegisterRoute("fontpage", typeof(Pages.Setting.FontPage));                  // フォント選択

            Routing.RegisterRoute("mypage", typeof(Pages.Setting.MyPage));                      // マイページ

            Routing.RegisterRoute("editprofilepage", typeof(Pages.Setting.EditProfilePage));    // 編集ページ
        }

        protected override async void OnAppearing() {

            base.OnAppearing();
        }
    }
}