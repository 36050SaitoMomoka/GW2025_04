using LocalDisasterPreventionInformationApp.Database;
using Microsoft.Maui.Controls;
using System.Globalization;
using System.Text.Json;
//using Windows.System;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

[QueryProperty(nameof(Name), "name")]
[QueryProperty(nameof(Furigana), "furigana")]
[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(Phone), "phone")]
[QueryProperty(nameof(Address), "address")]
[QueryProperty(nameof(PostalCode), "postal")]

public partial class ConfirmPage : ContentPage {
    private readonly AppDatabase _db;

    public string Name {
        set => nameLabel.Text = value;
    }

    public string Furigana {
        set => furiganaLabel.Text = value;
    }

    public string Email {
        set {
            emailLabel.Text = string.IsNullOrWhiteSpace(value) ? "---" : value;
        }
    }

    public string Phone {
        set {
            phoneLabel.Text = string.IsNullOrWhiteSpace(value) ? "---" : value;
        }
    }

    public string Address {
        get => addressLabel.Text;
        set => addressLabel.Text = value;
    }

    public string PostalCode { get; set; }

    public ConfirmPage(AppDatabase db) {
        InitializeComponent();
        _db = db;
    }

    //住所から緯度経度を求める
    private async Task<(double? lat, double? lon)> GetLatLngFromAddress(string address) {
        try {
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "LocalDisasterPreventionApp/1.0");

            var json = await client.GetStringAsync(url);

            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);

            if (results != null && results.Count > 0) {
                double lat = double.Parse(results[0].lat, CultureInfo.InvariantCulture);
                double lon = double.Parse(results[0].lon, CultureInfo.InvariantCulture);
                return (lat, lon);
            }
        }
        catch {
            // 失敗したら null を返す
        }

        return (null, null);
    }

    public class NominatimResult {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {

        //Userテーブルへ保存
        var user = new Models.User {
            Name = nameLabel.Text,
            Furigana = furiganaLabel.Text,
            PhoneNumber = phoneLabel.Text == "---" ? null : phoneLabel.Text,
            Email = emailLabel.Text == "---" ? null : emailLabel.Text
        };
        await _db.SaveUserAsync(user);

        //UserAddressテーブルへ保存
        var (lat, lon) = await GetLatLngFromAddress(Address);

        var address = new Models.UserAddress {
            AddressType = "自宅",
            PostalCode = PostalCode,
            Address = addressLabel.Text,
            Longitude = lat,
            Latitude = lon,
        };
        await _db.AddAddressIfNotExistsAsync(address);

        //Preferences.Set("IsRegistered", true);
        await Shell.Current.GoToAsync("//TopPage");

        // TEST データが入ったか確認
        await _db.RunUserDataChecks();
    }


    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("///RegisterPage");
    }
}