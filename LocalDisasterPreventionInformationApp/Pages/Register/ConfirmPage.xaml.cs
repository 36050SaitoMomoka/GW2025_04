using LocalDisasterPreventionInformationApp.Database;
using Microsoft.Maui.Controls;
//using Windows.System;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

[QueryProperty(nameof(Name), "name")]
[QueryProperty(nameof(Furigana), "furigana")]
[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(Phone), "phone")]
[QueryProperty(nameof(Address), "address")]
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
        set => addressLabel.Text = value;
    }

    public ConfirmPage(AppDatabase db) {
        InitializeComponent();
        _db = db;
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {

        //Userテーブルへ保存
        //var user = new Models.User {
        //    Name = nameLabel.Text,
        //    Furigana = furiganaLabel.Text,
        //    PhoneNumber = phoneLabel.Text == "---" ? null : phoneLabel.Text,
        //    Email = emailLabel.Text == "---" ? null : emailLabel.Text
        //};
        //await _db.SaveUserAsync(user);

        //UserAddressテーブルへ保存
        //var address = new Models.UserAddress {

        //};
        //await _db.AddAddressIfNotExistsAsync(address);

        Preferences.Set("IsRegistered", true);
        await Shell.Current.GoToAsync("//TopPage");
    }

    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("///RegisterPage");
    }
}