namespace LocalDisasterPreventionInformationApp {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();

            // トップページ
            Routing.RegisterRoute("top", typeof(Pages.Top.TopPage));

            // 災害速報
            Routing.RegisterRoute("disaster", typeof(Pages.Disaster.DisasterPage));

            // 友達一覧
            Routing.RegisterRoute("friends", typeof(Pages.Friends.FriendsPage));

            // チャット
            Routing.RegisterRoute("chat", typeof(Pages.Friends.ChatPage));

            // 安否ボタン一覧
            Routing.RegisterRoute("safety", typeof(Pages.Friends.SafetyListPage));

            // 備蓄管理
            Routing.RegisterRoute("stock", typeof(Pages.Stock.StockPage));

            // 商品登録
            Routing.RegisterRoute("product", typeof(Pages.Stock.ProductRegisterPage));

            // 多言語対応
            Routing.RegisterRoute("language", typeof(Pages.Language.LanguagePage));

            // マイページ
            Routing.RegisterRoute("mypage", typeof(Pages.My.MyPage));
        }
    }
}