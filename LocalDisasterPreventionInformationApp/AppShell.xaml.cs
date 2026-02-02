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
            

            CheckData(); //TEST
            CheckStockData(); //TEST

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

        //TEST
        private async void CheckData() {
            var products = await _db.GetProductsAsync();
            Debug.WriteLine($"Product 件数： {products.Count}");

            foreach (var p in products) {
                Debug.WriteLine($"ID = {p.ProductId}, Name = {p.Name}");
            }
        }

        //TEST
        private async void CheckStockData() {
            var stocks = await _db.GetStocksAsync();
            Debug.WriteLine($"Stock 件数： {stocks.Count}");

            foreach (var s in stocks) {
                Debug.WriteLine($"ID = {s.StockId}, pID = {s.ProductId}, Date = {s.ExpirationDate}, Qua = {s.Quantity}");
            }
        }

        protected override async void OnAppearing() {

            base.OnAppearing();

            if (_initialized)
                return;

            _initialized = true;

            //DBの初期化を待つ
            await _db.InitializeAsync();

            //TEST
            Preferences.Set("IsRegistered", false);

            bool isRegistered = Preferences.Get("IsRegistered", false);


            if (!isRegistered) {

                // 初回起動時だけ避難所データを読み込みデータ追加
                await _shelterService.FetchAndSaveShelterAsync();

                // RegisterPage へ遷移
                await GoToAsync("//RegisterPage");
            } else {
                // 2回目以降
                await GoToAsync("//TopPage");
            }
        }
    }
}