using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Text;
using Microsoft.Maui.Devices.Sensors;   // GPS
using System.Text.Json;
using System.Threading.Tasks;                 // JSON変換用

namespace LocalDisasterPreventionInformationApp.Pages.Top;
[QueryProperty(nameof(DoRouteSearch), "route")]
public partial class TopPage : ContentPage {
    public string DoRouteSearch { get; set; }

    private readonly AppDatabase _db;
    private List<NearbyShelterItem> _nearest10;
    private List<NearbyShelterItem> _currentList;

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        // WebView強制リロード
        MapWebView.Source = null;
        MapWebView.Source = new UrlWebViewSource {
            Url = "map.html?ts=" + DateTime.Now.Ticks
        };

        // PageTitle を設定
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }

        // 都道府県をPickerにセット
        await LoadPrefecturesAsync();

        // 初期状態では市区町村Pickerを押せないようにする
        CityPicker.IsEnabled = false;
    }

    private async void MapWebView_Navigated(object sender, WebNavigatedEventArgs e) {
        // 初回ロード時だけ地図を描写
        if (e.Url.Contains("map.html")) {
            // 現在地と避難所データを取得してleafletに渡す
            await LoadSheltersAndShowPinsAsync();

            // ルート検索フラグが立っていたら、このタイミングでだけ実行
            if (DoRouteSearch == "1") {
                await RouteToNearestShelterAsync(((AppShellViewModel)BindingContext).SelectedRouteMode);
                DoRouteSearch = null;
            }
        }
    }

    // 最寄り避難所へのルート検索
    public async Task RouteToNearestShelterAsync(string mode = "driving") {
        var location = await Geolocation.GetLocationAsync();
        if (location == null)
            return;

        double currentLat = location.Latitude;
        double currentLng = location.Longitude;

        if (_nearest10 == null || _nearest10.Count == 0)
            return;

        var nearest = _nearest10.OrderBy(s => s.Distance).First();
        double toLat = nearest.Shelter.Latitude;
        double toLng = nearest.Shelter.Longitude;

        await MapWebView.EvaluateJavaScriptAsync(
            $"showRoute({currentLat}, {currentLng}, {toLat}, {toLng}, '{mode}');");

        NearbySheltersList.SelectedItem = nearest;
    }

    // 都道府県読み込み
    private async Task LoadPrefecturesAsync() {
        var shelters = await _db.GetSheltersAsync();

        // 都道府県一覧を重複なしで取得
        var prefectures = shelters
            .Select(s => s.Prefecture)
            .Distinct()
            .ToList();

        PrefecturePicker.ItemsSource = prefectures;
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
        await MapWebView.EvaluateJavaScriptAsync($"addNearbyMarkers({json});");

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

            var current = NearbySheltersList.ItemsSource;
            NearbySheltersList.ItemsSource = null;
            NearbySheltersList.ItemsSource = current;
        }
    }

    public class NearbyShelterItem {
        public Shelter Shelter { get; set; }
        public double Distance { get; set; }
        public bool IsSelected { get; set; }
    }

    private async void PrefecturePicker_SelectedIndexChanged(object sender, EventArgs e) {

        // 都道府県が未選択なら市区町村 Picker を無効化
        if (PrefecturePicker.SelectedIndex < 0) {
            CityPicker.ItemsSource = null;
            CityPicker.IsEnabled = false;
            ExecuteButton.IsEnabled = false;
            ExitButton.IsEnabled = false;
            return;
        }

        ExecuteButton.IsEnabled = true;
        ExitButton.IsEnabled = true;

        if (PrefecturePicker.SelectedItem is string prefecture) {
            var shelters = await _db.GetSheltersAsync();

            var cities = shelters
                .Where(s => s.Prefecture == prefecture)
                .Select(s => s.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CityPicker.ItemsSource = cities;
            CityPicker.IsEnabled = true;   // 都道府県が選ばれたので有効化
        }
    }

    private void CityPicker_SelectedIndexChanged(object sender, EventArgs e) {
        ExecuteButton.IsEnabled = CityPicker.SelectedIndex >= 0;
    }

    private async void ExecuteButton_Clicked(object sender, TappedEventArgs e) {
        if (PrefecturePicker.SelectedItem is not string selectedPrefecture)
            return;

        // DBから選択された都道府県の避難所を取得
        var shelters = await _db.GetSheltersAsync();

        string prefecture = PrefecturePicker.SelectedItem as string;
        string city = CityPicker.SelectedItem as string;

        var filtered = shelters
            .Where(s => s.Prefecture == prefecture)
            .Where(s => city == null || s.City == city)
            .ToList();

        // 現在地を再取得
        var location = await Geolocation.GetLocationAsync();
        double currentLat = location.Latitude;
        double currentLng = location.Longitude;

        // 距離を計算
        foreach (var s in filtered) {
            s.Distance = HaversineDistance(currentLat, currentLng, s.Latitude, s.Longitude);
        }

        // リストを都道府県の避難所一覧に切り替える
        var listItems = filtered
            .OrderBy(s => s.Distance)
            .Select(s => new NearbyShelterItem {
                Shelter = s,
                Distance = s.Distance,
                IsSelected = false
            })
            .ToList();

        // リストを更新
        _currentList = listItems;
        NearbySheltersList.ItemsSource = listItems;

        // JSONに変換
        var json = JsonSerializer.Serialize(filtered);

        // WebViewにピン追加命令
        await MapWebView.EvaluateJavaScriptAsync($"addShelterMarkers({json});");
    }

    private async void ExitButton_Clicked(object sender, TappedEventArgs e) {
        // 都道府県ピンを削除
        await MapWebView.EvaluateJavaScriptAsync("clearPrefectureMarkers();");

        // 現在地付近の10件を再描画
        await LoadSheltersAndShowPinsAsync();

        // Pickerを未選択に戻す
        PrefecturePicker.SelectedIndex = -1;
        CityPicker.SelectedIndex = -1;

        // ボタンを無効化
        ExecuteButton.IsEnabled = false;
        ExitButton.IsEnabled = false;
    }
}