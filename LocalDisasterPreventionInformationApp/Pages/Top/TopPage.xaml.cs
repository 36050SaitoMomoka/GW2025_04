using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Text;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

public partial class TopPage : ContentPage {
    private readonly AppDatabase _db;
    private readonly List<Location> allLocations = new();

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        // 地域データ読み込みと初期化
        if (allLocations.Count == 0) {
            await LoadLocations();
            PopulatePrefectureComboBox();
        }

        // PageTitle を設定
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }
    }

    // ===== 地域データ読み込み =====
    private async Task LoadLocations() {
        try {
            using var stream = await FileSystem.OpenAppPackageFileAsync("locations.txt");
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;

                var loc = new Location {
                    Code = parts[0],
                    Prefecture = parts[1],
                    City = parts[2],
                    PrefectureKana = parts[3],
                    CityKana = parts[4],
                    Latitude = double.Parse(parts[5]),
                    Longitude = double.Parse(parts[6])
                };

                allLocations.Add(loc);
            }
        }
        catch (Exception ex) {
            await DisplayAlert("エラー", $"読み込みエラー: {ex.Message}", "OK");
        }
    }

    // ===== 都道府県コンボボックス初期化 =====
    private void PopulatePrefectureComboBox() {
        var prefectures = allLocations
            .Select(l => l.Prefecture)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        PrefecturePicker.ItemsSource = prefectures;
        PrefecturePicker.SelectedIndex = -1;

        CityPicker.ItemsSource = null;
        CityPicker.SelectedIndex = -1;
    }

    // ===== 都道府県選択イベント =====
    private void PrefecturePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (PrefecturePicker.SelectedItem is string selectedPrefecture) {
            var cities = allLocations
                .Where(l => l.Prefecture == selectedPrefecture)
                .Select(l => l.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CityPicker.ItemsSource = cities;
            CityPicker.SelectedIndex = -1;
        }
    }

    // ===== 検索ボタンイベント =====
    private async void SearchButton_Click(object sender, EventArgs e) {
        string prefecture = PrefecturePicker.SelectedItem as string ?? "";
        string city = CityPicker.SelectedItem as string ?? "";

        if (string.IsNullOrWhiteSpace(prefecture) || string.IsNullOrWhiteSpace(city)) {
            await DisplayAlert("エラー", "都道府県と市区町村を選択してください。", "OK");
            return;
        }

        var location = allLocations.FirstOrDefault(l => l.Prefecture == prefecture && l.City == city);
        if (location == null) {
            await DisplayAlert("エラー", "座標情報が見つかりません。locations.txt を確認してください。", "OK");
            return;
        }

        // Leaflet の地図を移動
        string jsMove = $"moveTo({location.Latitude}, {location.Longitude}, 13);";
        await MapWebView.EvaluateJavaScriptAsync(jsMove);

        // 避難所マーカー表示
        var shelters = (await _db.GetSheltersAsync())
            .Where(s => s.Prefecture == prefecture && s.City == city)
            .ToList();

        foreach (var s in shelters) {
            string jsAdd = $"addShelter({s.Latitude}, {s.Longitude}, '{s.Name}');";
            await MapWebView.EvaluateJavaScriptAsync(jsAdd);
        }
    }

    // 地域情報クラス
    public class Location {
        public string Code { get; set; } = "";
        public string Prefecture { get; set; } = "";
        public string City { get; set; } = "";
        public string PrefectureKana { get; set; } = "";
        public string CityKana { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}