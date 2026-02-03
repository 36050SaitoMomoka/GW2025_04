using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Text;
using Microsoft.Maui.Devices.Sensors;   // GPS
using System.Text.Json;
using System.Threading.Tasks;                 // JSON•ÏŠ·—p

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

        // PageTitle ‚ðÝ’è
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "ƒgƒbƒvƒy[ƒW";
        }

        // “s“¹•{Œ§‚ðPicker‚ÉƒZƒbƒg
        await LoadPrefecturesAsync();
    }

    private async void MapWebView_Navigated(object sender, WebNavigatedEventArgs e) {
        // ‰‰ñƒ[ƒhŽž‚¾‚¯’n}‚ð•`ŽÊ
        if (e.Url.Contains("map.html")) {
            // Œ»Ý’n‚Æ”ð“ïŠƒf[ƒ^‚ðŽæ“¾‚µ‚Äleaflet‚É“n‚·
            await LoadSheltersAndShowPinsAsync();
        }
    }

    // “s“¹•{Œ§“Ç‚Ýž‚Ý
    private async Task LoadPrefecturesAsync() {
        var shelters = await _db.GetSheltersAsync();

        // “s“¹•{Œ§ˆê——‚ðd•¡‚È‚µ‚ÅŽæ“¾
        var prefectures = shelters
            .Select(s => s.Prefecture)
            .Distinct()
            .ToList();

        PrefecturePicker.ItemsSource = prefectures;
    }

    //Œ»Ý’nŽæ“¾ ¨ DBŽæ“¾ ¨ ‹——£ŒvŽZ ¨ ãˆÊ10Œ ¨ leaflet‚É“n‚·
    private async Task LoadSheltersAndShowPinsAsync() {
        // GPS‚ÅŒ»Ý’n‚ðŽæ“¾
        var location = await Geolocation.GetLocationAsync();
        double currentLat = location.Latitude;
        double currentLng = location.Longitude;

        // DB‚©‚ç”ð“ïŠˆê——‚ðŽæ“¾
        var shelters = await _db.GetSheltersAsync();

        // Œ»Ý’n‚Æ‚Ì‹——£‚ðŒvŽZ
        foreach (var s in shelters) {
            s.Distance = HaversineDistance(currentLat, currentLng, s.Latitude, s.Longitude);
        }

        // ‹——£‚ª‹ß‚¢‡‚ÉãˆÊ10Œ‚ðŽæ“¾
        var nearest10 = shelters.OrderBy(s => s.Distance).Take(10).ToList();

        // _nearest10‚É•ÏŠ·‚µ‚Ä•ÛŽ
        _nearest10 = nearest10
            .Select(s => new NearbyShelterItem {
                Shelter = s,
                Distance = s.Distance,
                IsSelected = false
            })
            .ToList();

        // Œ»Ý’n‚ðleaflet‚É‘—‚é
        await MapWebView.EvaluateJavaScriptAsync($"setCurrentLocation({currentLat},{currentLng});");

        // JSON•ÏŠ·
        var json = JsonSerializer.Serialize(nearest10);

        // WebView‚Éƒsƒ“’Ç‰Á–½—ß‚ð‘—‚é
        await MapWebView.EvaluateJavaScriptAsync($"addShelterMarkers({json});");

        // 10ŒƒŠƒXƒg
        NearbySheltersList.ItemsSource = _nearest10;
    }

    // ‹——£ŒvŽZ
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2) {
        double R = 6371; // ’n‹…‚Ì”¼Œaikmj
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

            // ’n}‚ðˆÚ“®
            await MapWebView.EvaluateJavaScriptAsync($"moveToShelter({lat},{lng});");

            // ƒŠƒXƒg‚Ì”wŒiF‚ðXV
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

    private void PrefecturePicker_SelectedIndexChanged(object sender, EventArgs e) {
        ExecuteButton.IsEnabled = PrefecturePicker.SelectedIndex >= 0;
        ExitButton.IsEnabled = PrefecturePicker.SelectedIndex >= 0;
    }

    private async void ExecuteButton_Clicked(object sender, TappedEventArgs e) {
        if (PrefecturePicker.SelectedItem is not string selectedPrefecture)
            return;

        // DB‚©‚ç‘I‘ð‚³‚ê‚½“s“¹•{Œ§‚Ì”ð“ïŠ‚ðŽæ“¾
        var shelters = await _db.GetSheltersAsync();
        var filtered = shelters
            .Where(s => s.Prefecture == selectedPrefecture)
            .ToList();

        // JSON‚É•ÏŠ·
        var json = JsonSerializer.Serialize(filtered);

        // WebView‚Éƒsƒ“’Ç‰Á–½—ß
        await MapWebView.EvaluateJavaScriptAsync($"addShelterMarkers({json});");
    }

    private void ExitButton_Clicked(object sender, TappedEventArgs e) {

    }
}