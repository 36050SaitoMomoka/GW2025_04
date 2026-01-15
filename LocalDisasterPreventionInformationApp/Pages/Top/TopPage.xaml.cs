using LocalDisasterPreventionInformationApp.Models;   // ← Yahoo / 避難所レスポンスモデル用
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.Text.Json;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

public partial class TopPage : ContentPage {

    private readonly List<string> newsItems = new(){
        "群馬県で震度5弱の地震が発生しました。",
        "太田市内の避難所が開設されました。",
        "気象庁が余震に注意を呼びかけています。"
    };

    private int newsIndex = 0;
    private bool isRunning = false;

    public TopPage() {
        InitializeComponent();

        double s = Responsive.Scale;

        // ハンバーガーメニュー
        menuButton.WidthRequest *= s;
        menuButton.HeightRequest *= s;

        // タイトル
        selectedMenuLabel.FontSize *= s;

        // 右上アイコン（HorizontalStackLayout の子）
        foreach (var child in topRightIcons.Children) {
            if (child is ImageButton ib) {
                ib.WidthRequest *= s;
                ib.HeightRequest *= s;
            }
        }

        // ルート検索ボタン
        routeSearchButton.FontSize *= s;
        routeSearchButton.WidthRequest *= s;
        routeSearchButton.HeightRequest *= s;

        // ニュースラベル
        newsLabel.FontSize *= s;
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        isRunning = true;
        StartNewsCycle();
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        isRunning = false;
    }

    private void StartNewsCycle() {
        RunNewsAnimation();
    }

    private void RunNewsAnimation() {
        if (!isRunning)
            return;

        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(5), async () => {
            if (!isRunning)
                return;

            newsLabel.Opacity = 0;
            newsLabel.Text = newsItems[newsIndex];
            await newsLabel.FadeTo(1, 1000);

            newsIndex = (newsIndex + 1) % newsItems.Count;

            RunNewsAnimation();
        });
    }

    private void OnMenuButtonClicked(object sender, EventArgs e) {
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void OnChatClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Friends.ChatPage));
    }

    private async void OnNotificationClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Notification.NotificationPage));
    }

    private async void OnSettingsClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync(nameof(Setting.SettingPage));
    }

    private async void OnRouteSearchClicked(object sender, EventArgs e) {
        try {
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High)
            );

            if (location == null) {
                await DisplayAlert("エラー", "現在地を取得できませんでした", "OK");
                return;
            }

            double lat = location.Latitude;
            double lon = location.Longitude;

            var client = new HttpClient();
            string nearestUrl =
                $"https://disaportaldata.gsi.go.jp/api/hinanbasho/nearest?lat={lat}&lon={lon}";

            var nearestJson = await client.GetStringAsync(nearestUrl);
            var shelter = JsonSerializer.Deserialize<ShelterResponse>(nearestJson);

            double shelterLat = shelter.location.lat;
            double shelterLon = shelter.location.lon;

            string yahooUrl =
                $"https://map.yahooapis.jp/course/V1/route?appid={Constants.YahooAppId}" +
                $"&start={lat},{lon}&goal={shelterLat},{shelterLon}&mode=car&output=json";

            var routeJson = await client.GetStringAsync(yahooUrl);
            var route = JsonSerializer.Deserialize<YahooRouteResponse>(routeJson);

            var coords = route.Feature[0].Geometry.Coordinates;

            string jsArray = JsonSerializer.Serialize(coords);
            await mapWebView.EvaluateJavaScriptAsync($"drawRoute({jsArray})");
        }
        catch (Exception ex) {
            await DisplayAlert("エラー", ex.Message, "OK");
        }
    }
}