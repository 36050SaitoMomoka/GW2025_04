using LocalDisasterPreventionInformationApp.Services;

namespace LocalDisasterPreventionInformationApp {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();

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
    }
}