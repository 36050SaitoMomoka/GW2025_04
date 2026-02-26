using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using Microsoft.Maui.Devices.Sensors;   // GPS
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;                 // JSON変換用

namespace LocalDisasterPreventionInformationApp.Pages.Top;
[QueryProperty(nameof(DoRouteSearch), "route")]
public partial class TopPage : ContentPage {
    public string DoRouteSearch { get; set; }

    private readonly AppDatabase _db;
    private List<NearbyShelterItem> _nearest10;
    private List<NearbyShelterItem> _currentList;

    public bool _isMapLoaded = false;
    private string _pendingRouteMode = null;

    private double? _pendingLat = null;
    private double? _pendingLng = null;

    private WebView MapWebView;

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        //ここから追加
        MapWebView = new WebView {
#if ANDROID
            Source = new UrlWebViewSource {
                Url = $"file:///android_asset/map.html?cb={Guid.NewGuid()}"

            }
#else
            Source = "map.html"
#endif
        };

        MapWebView.Navigated += MapWebView_Navigated;
        Inner.Children.Insert(0,MapWebView);



        BindingContext = Shell.Current.BindingContext;


        // RouteModeChangedイベントを取得
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = vm.Header_Top;

            // 言語切り替え時にも Picker を更新
            vm.PropertyChanged += (s, e) => {
                if (e.PropertyName == null || e.PropertyName == "SelectedLanguage") {
                    vm.PageTitle = vm.Header_Top;
                }
            };
        }

        // 移動手段ボタンからの呼び出し
        vm.RouteModeChanged += async (mode) =>
        {
            await WaitForMapReadyAsync();

            if (!_isMapLoaded) {
                _pendingRouteMode = mode;
                return;
            }

            // 選択中の避難所がある場合 → その避難所へルート検索
            if (_selectedItem != null) {
                var s = _selectedItem.Shelter;

                string js = $"showRouteFromCurrent({s.Latitude.ToString(CultureInfo.InvariantCulture)}, " +
                            $"{s.Longitude.ToString(CultureInfo.InvariantCulture)}, '{mode}');";

                await MapWebView.EvaluateJavaScriptAsync(js);
                return;
            }

            // 選択されていない場合 → 最寄り避難所へ
            if (_nearest10?.Count > 0) {
                await RouteToNearestShelterAsync(mode);
            }
        };
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        // WebView の内部キャッシュを完全に破棄
        //MapWebView.Reload();

        // 必ず Navigated が発火する URL にする
        //MapWebView.Source = new UrlWebViewSource {
        //    Url = $"file:///android_asset/map.html?cb={Guid.NewGuid()}"
        //};

        // PageTitle を設定
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = vm.Header_Top;
        }

        // 都道府県をPickerにセット
        await LoadPrefecturesAsync();

        // 初期状態では市区町村Pickerを押せないようにする
        CityPicker.IsEnabled = false;
    }

    private async void MapWebView_Navigated(object sender, WebNavigatedEventArgs e) {
        if (e.Url.Contains("map.html")) {
            _isMapLoaded = true;

            await WaitForMapReadyAsync();

            if(_pendingLat != null && _pendingLng != null) {
                await MapWebView.EvaluateJavaScriptAsync(
                    $"setCurrentLocation({_pendingLat},{_pendingLng});"
                );
            }

            // 現在地と避難所データを取得してleafletに渡す
            await LoadSheltersAndShowPinsAsync();

            // ルート検索フラグが立っていたら、このタイミングでだけ実行
            if (DoRouteSearch == "1") {
                await RouteToNearestShelterAsync("driving");
                DoRouteSearch = null;
            }
        }
    }

    private async Task WaitForMapReadyAsync() {
        for(int i = 0; i < 50; i++) {
            var result = await MapWebView.EvaluateJavaScriptAsync("window.mapReady");
            if (result == "true") return;

            await Task.Delay(100);
        }
    }

    // 最寄り避難所へのルート検索
    public async Task RouteToNearestShelterAsync(string mode = "driving") {
        var location = await Geolocation.GetLocationAsync();
        if (location == null)
            return;

        if (!_isMapLoaded) return;

        await WaitForMapReadyAsync();

        double currentLat = location.Latitude;
        double currentLng = location.Longitude;

        if (_nearest10 == null || _nearest10.Count == 0)
            return;

        var nearest = _nearest10.OrderBy(s => s.Distance).First();
        double toLat = nearest.Shelter.Latitude;
        double toLng = nearest.Shelter.Longitude;

        // JS に渡す値は小数点付きで固定
        string js = $"showRoute({currentLat.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{currentLng.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{toLat.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{toLng.ToString(CultureInfo.InvariantCulture)}, " +
                    $"'{mode}');";

        await MapWebView.EvaluateJavaScriptAsync(js);

        NearbySheltersList.SelectedItem = nearest;
        _selectedItem = nearest;
    }

    // リスト対応ルート検索
    private NearbyShelterItem _selectedItem;

    public async Task RouteSearchBasedOnSelectionAsync() {
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        string mode = vm?.CurrentRouteMode ?? "driving";

        // 選択されている場合 → その避難所へ
        if (_selectedItem != null) {
            var s = _selectedItem.Shelter;

            string js = $"showRouteFromCurrent({s.Latitude.ToString(CultureInfo.InvariantCulture)}, " +
                        $"{s.Longitude.ToString(CultureInfo.InvariantCulture)}, '{mode}');";

            await MapWebView.EvaluateJavaScriptAsync(js);
            return;
        }

        // 選択されていない場合 → 最寄り避難所へ
        if (_nearest10 != null && _nearest10.Count > 0) {
            var nearest = _nearest10.OrderBy(s => s.Distance).First();
            var s = nearest.Shelter;

            string js = $"showRouteFromCurrent({s.Latitude.ToString(CultureInfo.InvariantCulture)}, " +
                        $"{s.Longitude.ToString(CultureInfo.InvariantCulture)}, '{mode}');";

            await MapWebView.EvaluateJavaScriptAsync(js);

            NearbySheltersList.SelectedItem = nearest;
            _selectedItem = nearest;
        }
    }

    // ルート削除
    public async Task ClearRouteAsync() {
        if (!_isMapLoaded) return;

        await WaitForMapReadyAsync();

        await MapWebView.EvaluateJavaScriptAsync("clearRoute();");
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
        await WaitForMapReadyAsync();

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
            _selectedItem = item;

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
        NearbySheltersList.SelectedItem = null;
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
        await WaitForMapReadyAsync();

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
        await WaitForMapReadyAsync();

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