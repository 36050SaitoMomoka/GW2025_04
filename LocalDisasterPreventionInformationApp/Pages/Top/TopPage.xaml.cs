using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

//ContentPageを継承
public partial class TopPage : ContentPage {

    private readonly AppDatabase _db;
    private readonly List<Location> allLocations = new();

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        BindingContext = new LocationViewModel();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        // 地域データ読み込みと初期化
        //await LoadLocations();
        //PopulatePrefectureComboBox();

        //PageTitleを「トップページ」にする
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }
    }

    public class Location {
        public string Code { get; set; }
        public string Prefecture { get; set; }
        public string City { get; set; }
    }

    public class LocationService {
        public List<Location> LoadLocations(string filePath) {
            var list = new List<Location>();

            foreach (var line in File.ReadLines(filePath)) {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2) {
                    var code = parts[0];
                    var prefecture = parts[1];
                    var city = parts.Length >= 3 ? parts[2] : null;

                    list.Add(new Location {
                        Code = code,
                        Prefecture = prefecture,
                        City = city
                    });
                }
            }

            return list;
        }
    }

    public class LocationViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Location> _allLocations;

        public ObservableCollection<string> Prefectures { get; } = new();
        public ObservableCollection<string> Cities { get; } = new();

        private string _selectedPrefecture;
        public string SelectedPrefecture {
            get => _selectedPrefecture;
            set {
                if (_selectedPrefecture != value) {
                    _selectedPrefecture = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPrefecture)));
                    UpdateCities();
                }
            }
        }

        public string SelectedCity { get; set; }

        public LocationViewModel() {
            var service = new LocationService();
            _allLocations = service.LoadLocations("locations.txt");

            // 都道府県一覧をセット
            foreach (var pref in _allLocations
                .Where(x => x.City == null)
                .Select(x => x.Prefecture)
                .Distinct()) {
                Prefectures.Add(pref);
            }
        }

        private void UpdateCities() {
            Cities.Clear();

            var cities = _allLocations
                .Where(x => x.Prefecture == SelectedPrefecture && x.City != null)
                .Select(x => x.City)
                .Distinct();

            foreach (var city in cities) {
                Cities.Add(city);
            }
        }
    }

    //// ===== 地域データ読み込み =====
    //private async Task LoadLocations() {
    //    try {
    //        using var stream = await FileSystem.OpenAppPackageFileAsync("locations.txt");
    //        using var reader = new StreamReader(stream, Encoding.UTF8);

    //        string? line;
    //        while ((line = await reader.ReadLineAsync()) != null) {
    //            if (string.IsNullOrWhiteSpace(line)) continue;

    //            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    //            if (parts.Length < 5) continue;

    //            var loc = new Location {
    //                Code = parts[0],
    //                Prefecture = parts[1],
    //                City = parts[2],
    //                PrefectureKana = parts[3],
    //                CityKana = parts[4],
    //            };

    //            allLocations.Add(loc);
    //        }
    //    }
    //    catch (Exception ex) {
    //        await DisplayAlert("エラー", $"読み込みエラー: {ex.Message}", "OK");
    //    }
    //}
    //// ===== 都道府県コンボボックス初期化 =====
    //private void PopulatePrefectureComboBox() {
    //    var prefectures = allLocations.Select(l => l.Prefecture)
    //                                  .Where(p => !string.IsNullOrWhiteSpace(p))
    //                                  .Distinct()
    //                                  .ToList();

    //    PrefecturePicker.ItemsSource = prefectures;
    //    PrefecturePicker.SelectedIndex = -1;

    //    CityPicker.ItemsSource = null;
    //    CityPicker.SelectedIndex = -1;
    //}

    //// ===== 都道府県選択イベント =====
    //private void PrefecturePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    //    if (PrefecturePicker.SelectedItem is string selectedPrefecture) {
    //        var cities = allLocations.Where(l => l.Prefecture == selectedPrefecture)
    //                                 .Select(l => l.City)
    //                                 .Where(c => !string.IsNullOrWhiteSpace(c))
    //                                 .Distinct()
    //                                 .ToList();

    //        CityPicker.ItemsSource = cities;
    //        CityPicker.SelectedIndex = -1;
    //    }
    //}

    //// ===== 検索ボタンイベント =====
    //private async void SearchButton_Click(object sender, EventArgs e) {
    //    string prefecture = PrefecturePicker.SelectedItem as string ?? "";
    //    string city = CityPicker.SelectedItem as string ?? "";

    //    if (string.IsNullOrWhiteSpace(prefecture))
    //        prefecture = (PrefecturePicker.SelectedItem as string)?.Trim() ?? "";
    //    if (string.IsNullOrWhiteSpace(city))
    //        city = (CityPicker.SelectedItem as string)?.Trim() ?? "";

    //    if (string.IsNullOrWhiteSpace(prefecture) || string.IsNullOrWhiteSpace(city)) {
    //        await DisplayAlert("エラー", "都道府県と市区町村を選択してください。", "OK");
    //        return;
    //    }

    //    var location = allLocations.FirstOrDefault(l => l.Prefecture == prefecture && l.City == city);
    //    if (location == null) {
    //        await DisplayAlert("エラー", "座標情報が見つかりません。locations.txt を確認してください。", "OK");
    //        return;
    //    }
    //}

    // 地域情報を保持するクラス
    //public class Location {
    //    public string Code { get; set; } = "";
    //    public string Prefecture { get; set; } = "";
    //    public string City { get; set; } = "";
    //    public string PrefectureKana { get; set; } = "";
    //    public string CityKana { get; set; } = "";
    //}

    //// Leaflet の地図を市区町村の中心へ移動
    //string jsMove = $"moveTo({location.Latitude}, {location.Longitude}, 13);";
    //await MapWebView.EvaluateJavaScriptAsync(jsMove);

    //// 市区町村内の避難所を DB から取得してマーカー表示
    //var shelters = (await _db.GetSheltersAsync())
    //    .Where(s => s.Prefecture == prefecture && s.City == city)
    //    .ToList();

    //foreach (var s in shelters) {
    //    string jsAdd = $"addShelter({s.Latitude}, {s.Longitude}, '{s.Name}');";
    //    await MapWebView.EvaluateJavaScriptAsync(jsAdd);
}