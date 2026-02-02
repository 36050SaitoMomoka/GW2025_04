using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Text;
using Microsoft.Maui.Devices.Sensors;   // GPS
using System.Text.Json;
using System.Threading.Tasks;                 // JSON変換用

namespace LocalDisasterPreventionInformationApp.Pages.Top;

public partial class TopPage : ContentPage {
    private readonly AppDatabase _db;
    private List<NearbyShelterItem> _nearest10;

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        // PageTitle を設定
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }
    }

    private async void MapWebView_Navigated(object sender, WebNavigatedEventArgs e) {
        // 初回ロード時だけ地図を描写
        if (e.Url.Contains("map.html")) {
            // 現在地と避難所データを取得してleafletに渡す
            await LoadSheltersAndShowPinsAsync();
        }
    }

    //現在地取得 → DB取得 → 距離計算 → 上位10件 → leafletに渡す
    private async Task LoadSheltersAndShowPinsAsync() {
        // GPSで現在地を取得
        var location = await Geolocation.GetLocationAsync();
        double currentLat = location.Latitude;
        double currentLng = location.Longitude;

        // DBから避難所一覧を取得
        var shelters = await _db.GetSheltersAsync();

        // 現在地との距離を計算
        foreach (var s in shelters) {
            s.Distance = HaversineDistance(currentLat, currentLng, s.Latitude, s.Longitude);
        }

        // 距離が近い順に上位10件を取得
        var nearest10 = shelters.OrderBy(s => s.Distance).Take(10).ToList();

        // _nearest10に変換して保持
        _nearest10 = nearest10
            .Select(s => new NearbyShelterItem {
                Shelter = s,
                Distance = s.Distance,
                IsSelected = false
            })
            .ToList();

        // 現在地をleafletに送る
        await MapWebView.EvaluateJavaScriptAsync($"setCurrentLocation({currentLat},{currentLng});");

        // JSON変換
        var json = JsonSerializer.Serialize(nearest10);

        // WebViewにピン追加命令を送る
        await MapWebView.EvaluateJavaScriptAsync($"addShelterMarkers({json});");

        // 10件リスト
        NearbySheltersList.ItemsSource = _nearest10;
    }

    // 距離計算
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2) {
        double R = 6371; // 地球の半径（km）
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) *
            Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private async void NearbySheltersList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.CurrentSelection.FirstOrDefault() is NearbyShelterItem item) {
            double lat = item.Shelter.Latitude;
            double lng = item.Shelter.Longitude;

            // 地図を移動
            await MapWebView.EvaluateJavaScriptAsync($"moveToShelter({lat},{lng});");

            // リストの背景色を更新
            foreach (var s in _nearest10)
                s.IsSelected = (s == item);

            NearbySheltersList.ItemsSource = null;
            NearbySheltersList.ItemsSource = _nearest10;
        }
    }

    public class NearbyShelterItem {
        public Shelter Shelter { get; set; }
        public double Distance { get; set; }
        public bool IsSelected { get; set; }
    }

}