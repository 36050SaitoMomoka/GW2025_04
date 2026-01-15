namespace LocalDisasterPreventionInformationApp {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();

            // チャット
            Routing.RegisterRoute("chat", typeof(Pages.Friends.ChatPage));

            // 安否ボタン一覧
            Routing.RegisterRoute("safety", typeof(Pages.Friends.SafetyListPage));

            // 商品登録
            Routing.RegisterRoute("product", typeof(Pages.Stock.ProductRegisterPage));

            // 多言語対応
            Routing.RegisterRoute("language", typeof(Pages.Language.LanguagePage));

            // マイページ
            Routing.RegisterRoute("mypage", typeof(Pages.My.MyPage));

            //設定
            Routing.RegisterRoute("setting", typeof(Pages.Setting.SettingPage));
        }
    }
}