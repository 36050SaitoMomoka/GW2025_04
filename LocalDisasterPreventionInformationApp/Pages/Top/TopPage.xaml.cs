using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Text;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

public partial class TopPage : ContentPage {
    private readonly AppDatabase _db;

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        // Shift-JIS読み込み対応
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Pickerイベント
        PrefecturePicker.SelectedIndexChanged += OnPrefectureChanged;
        CityPicker.SelectedIndexChanged += OnCityChanged;

        ExecuteButton.IsEnabled = false;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        // PageTitle を設定
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }
    }

    // DBから都道府県一覧を取得
    private async Task LoadPrefecturesAsync() {
        var shelters = await _db.GetSheltersAsync();

        //都道府県一覧をセット
        var prefectures = shelters
            .Select(x => x.Prefecture)
            .Distinct()
            .ToList();

        PrefecturePicker.ItemsSource = prefectures;
    }

    //都道府県選択
    private async void OnPrefectureChanged(object sender, EventArgs e) {
        string prefecture = PrefecturePicker.SelectedItem?.ToString();
        if (prefecture == null)
            return;

        var shelters = await _db.GetSheltersAsync();

        var cities = shelters
            .Where(s => s.Prefecture == prefecture)
            .Select(s => s.City)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        CityPicker.ItemsSource = cities;
        CityPicker.SelectedIndex = -1;

        UpdateExecuteButtonState();
    }

    // 市区町村選択
    private void OnCityChanged(object sender, EventArgs e) {
        UpdateExecuteButtonState();
    }

    private void UpdateExecuteButtonState() {
        ExecuteButton.IsEnabled =
            PrefecturePicker.SelectedIndex >= 0 &&
            CityPicker.SelectedIndex >= 0;
    }

    //実行　→　ShelterDB　→Leaflet
    private async void OnExecuteClicked(object sender, EventArgs e) {
        string prefecture = PrefecturePicker.SelectedItem?.ToString();
        string city = CityPicker.SelectedItem?.ToString();

        if (prefecture == null || city == null)
            return;

        // Shelter DBから該当避難所を取得
        var shelters = await _db.GetSheltersAsync();
        var targetShelters = shelters
            .Where(s => s.Prefecture == prefecture && s.City == city)
            .ToList();

        if(targetShelters.Count == 0) {
            await DisplayAlert("情報なし", "避難所が見つかりませんでした。", "OK");
            return;
        }

        //マーカー追加
        foreach (var s in targetShelters) {
            string js = $"addShelterMarker({s.Latitude},{s.Longitude},'{EscapeJs(s.Name)}');";
            await MapWebView.EvaluateJavaScriptAsync(js);
        }

        //最初の避難所へ移動
        var first = targetShelters.First();
        string moveJs = $"moveToLocation({first.Latitude},{first.Longitude});";
        await MapWebView.EvaluateJavaScriptAsync(moveJs);
    }

    private static string EscapeJs(string text)
        => text.Replace("", "\\'");
}