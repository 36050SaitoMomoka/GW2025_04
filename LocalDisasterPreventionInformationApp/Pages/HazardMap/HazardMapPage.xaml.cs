using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Pages.Hazard;

//ContentPageを継承
public partial class HazardMapPage : ContentPage {
    private readonly AppDatabase _db;

    //災害リスト
    private List<HazardType> _hazardList;

    //住所リスト
    private List<Models.UserAddress> _addressList;

    public HazardMapPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        LoadHazardTypes();
        LoadAddresses();

        BindingContext = Shell.Current.BindingContext;

        //PageTitleを「ハザードマップ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "ハザードマップ";
        }
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        try {
            // 現在地を取得
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5))
            );

            if (location != null) {
                // WebView が読み込み済みなら現在地へ移動
                if (_isWebViewReady) {
                    await HazardWebView.EvaluateJavaScriptAsync(
                        $"moveTo({location.Latitude}, {location.Longitude});"
                    );
                } else {
                    // WebView がまだなら後で使うために保存
                    _pendingLat = location.Latitude;
                    _pendingLng = location.Longitude;
                }
            }
        }
        catch {
            // 現在地が取れないときは東京駅
            _pendingLat = 35.681236;
            _pendingLng = 139.767125;
        }
    }

    //災害種別の読み込み
    private void LoadHazardTypes() {
        _hazardList = new List<HazardType> {
            new HazardType { Name = "洪水", TileUrl = "https://disaportaldata.gsi.go.jp/raster/01_flood_l2_shinsuishin/{z}/{x}/{y}.png"},
            new HazardType { Name = "津波", TileUrl = "https://disaportaldata.gsi.go.jp/raster/04_tsunami_newlegend_data/{z}/{x}/{y}.png" },
            new HazardType { Name = "高潮", TileUrl = "https://disaportaldata.gsi.go.jp/raster/03_hightide_l2_shinsuishin_data/{z}/{x}/{y}.png"},
            new HazardType { Name = "土砂災害", TileUrl = "https://disaportaldata.gsi.go.jp/raster/05_dosekiryukeikaikuiki/{z}/{x}/{y}.png"},
        };

        HazardPicker.ItemsSource = _hazardList;
    }

    //DBから住所を読み込む
    private async void LoadAddresses() {
        _addressList = await _db.GetAddressesAsync();
        AddressPicker.ItemsSource = _addressList;
    }

    //災害種類または住所が選ばれたらハザード更新
    private void OnSelectionChanged(object sender,EventArgs e) {
        var hazard = HazardPicker.SelectedItem as HazardType;
        var address = AddressPicker.SelectedItem as Models.UserAddress;

        if (hazard == null || address == null) return;

        UpdateMap(hazard.TileUrl, address.Latitude ?? 35.0, address.Longitude ?? 135.0);
    }

    //地図更新
    private async void UpdateMap(string tileUrl,double lat,double lon) {
        string js1 = "setHazardTile(\"" + tileUrl + "\");";
        await HazardWebView.EvaluateJavaScriptAsync(js1);

        string js2 = $"moveTo({lat},{lon});";
        await HazardWebView.EvaluateJavaScriptAsync(js2);
    }

    public class HazardType {
        public string Name { get; set; }
        public string TileUrl { get; set; }
    }

    private bool _isWebViewReady = false;
    private double? _pendingLat = null;
    private double? _pendingLng = null;

    private async void HazardWebView_Navigated(object sender, WebNavigatedEventArgs e) {
        _isWebViewReady = true;

        // OnAppearing で取得した現在地があればここで移動
        if (_pendingLat != null && _pendingLng != null) {
            await HazardWebView.EvaluateJavaScriptAsync(
                $"moveTo({_pendingLat}, {_pendingLng});"
            );
        }
    }

}
