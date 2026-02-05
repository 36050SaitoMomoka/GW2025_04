using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

[QueryProperty(nameof(Name), "name")]
[QueryProperty(nameof(Furigana), "furigana")]
[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(Phone), "phone")]
[QueryProperty(nameof(Address), "address")]
public partial class ConfirmPage : ContentPage {
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

    public ConfirmPage() {
        InitializeComponent();
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {
        Preferences.Set("IsRegistered", true);
        await Shell.Current.GoToAsync("//TopPage");
    }

    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("///RegisterPage");
    }
}