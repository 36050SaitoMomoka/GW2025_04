using Microsoft.Maui.Controls;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

[QueryProperty(nameof(Name), "name")]
[QueryProperty(nameof(Furigana), "furigana")]
[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(Phone), "phone")]
[QueryProperty(nameof(Address), "address")]
public partial class ConfirmPage : ContentPage {
    public string Name {
        set => nameLabel.Text = $"氏名：{value}";
    }

    public string Furigana {
        set => furiganaLabel.Text = $"フリガナ：{value}";
    }

    public string Email {
        set => emailLabel.Text = $"メールアドレス：{value}";
    }

    public string Phone {
        set => phoneLabel.Text = $"電話番号：{value}";
    }

    public string Address {
        set => addressLabel.Text = $"住所：{value}";
    }

    public ConfirmPage() {
        InitializeComponent();
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//TopPage");
    }

    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("///RegisterPage");
    }
}