using LocalDisasterPreventionInformationApp.Models;   // ← Yahoo / 避難所レスポンスモデル用
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.Text.Json;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

public partial class TopPage : ContentPage {

    //ニュースとして表示するテキスト一覧（仮）→Yahooなどからとってくるように修正
    private readonly List<string> newsItems = new(){
        "群馬県で震度5弱の地震が発生しました。",
        "太田市内の避難所が開設されました。",
        "気象庁が余震に注意を呼びかけています。"
    };

    private int newsIndex = 0;          //現在表示しているニュースのインデックス
    private bool isRunning = false;     //ニュースの自動切り替えが動作中かどうか

    public TopPage() {
        InitializeComponent();          //XAMLの初期化
    }

    //ニュースの切り替え開始
    protected override void OnAppearing() {
        base.OnAppearing();
        isRunning = true;
        StartNewsCycle();
    }

    //ニュースの切り替え停止
    protected override void OnDisappearing() {
        base.OnDisappearing();
        isRunning = false;
    }

    //ニュースの切り替え処理を開始
    private void StartNewsCycle() {
        RunNewsAnimation();
    }

    //５秒ごとにニュースを切り替える
    private void RunNewsAnimation() {
        if (!isRunning)
            return;

        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(5), async () => {
            if (!isRunning)
                return;

            newsLabel.Opacity = 0;                      //フェードアウト
            newsLabel.Text = newsItems[newsIndex];      //次のニュースに変更
            await newsLabel.FadeTo(1, 1000);            //フェードイン

            newsIndex = (newsIndex + 1) % newsItems.Count;      //次のインデックスへ

            RunNewsAnimation();         //次の切り替えへ
        });
    }

    //選択されたメニュー名を表示
    private void OnMenuButtonClicked(object sender, EventArgs e) {
        if (sender is MenuFlyoutItem item) {
            selectedMenuLabel.Text = item.Text;

            switch (item.Text) {
                case "トップページ":
                    break;
                case "災害速報":
                    break;
                case "ハザードマップ":
                    break;
            }
        }
    }

    //チャット画面へ
    private async void OnChatClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Friends.ChatPage));
    }

    //通知画面へ
    private async void OnNotificationClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Notification.NotificationPage));
    }

    //設定画面へ
    private async void OnSettingsClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Setting.SettingPage));
    }

    // ===============================
    //  ルート検索（Yahoo → Leaflet）
    // ===============================
    private async void OnRouteSearchClicked(object sender, EventArgs e) {
        try {
            // 現在地取得（GPS)
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High)
            );

            if (location == null) {
                await DisplayAlert("エラー", "現在地を取得できませんでした", "OK");
                return;
            }

            double lat = location.Latitude;     //現在地の緯度
            double lon = location.Longitude;    //現在地の経度

            // 最寄り避難所検索（全国地理院 API）
            var client = new HttpClient();
            string nearestUrl =
                $"https://disaportaldata.gsi.go.jp/api/hinanbasho/nearest?lat={lat}&lon={lon}";

            var nearestJson = await client.GetStringAsync(nearestUrl);
            var shelter = JsonSerializer.Deserialize<ShelterResponse>(nearestJson);

            double shelterLat = shelter.location.lat;       //避難所の緯度
            double shelterLon = shelter.location.lon;       //避難所の経度

            // 経路情報を取得（Yahoo! ルート検索 API）
            string yahooUrl =
                $"https://map.yahooapis.jp/course/V1/route?appid={Constants.YahooAppId}" +
                $"&start={lat},{lon}&goal={shelterLat},{shelterLon}&mode=car&output=json";

            var routeJson = await client.GetStringAsync(yahooUrl);
            var route = JsonSerializer.Deserialize<YahooRouteResponse>(routeJson);

            // Yahoo の座標列（lon,lat の配列）
            var coords = route.Feature[0].Geometry.Coordinates;

            // 4. Leaflet に渡す（WebView）
            string jsArray = JsonSerializer.Serialize(coords);
            await mapWebView.EvaluateJavaScriptAsync($"drawRoute({jsArray})");
        }
        catch (Exception ex) {
            await DisplayAlert("エラー", ex.Message, "OK");
        }
    }
}