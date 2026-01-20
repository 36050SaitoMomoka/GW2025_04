namespace LocalDisasterPreventionInformationApp.Pages.Register;

public partial class RegisterPage : ContentPage {
    public RegisterPage() {
        InitializeComponent();
    }

    private async void OnNextClicked(object sender, EventArgs e) {
        string name = nameEntry.Text?.Trim();
        string furigana = furiganaEntry.Text?.Trim();
        string email = emailEntry.Text?.Trim();
        string phone = phoneEntry.Text?.Trim();
        string address = addressEntry.Text?.Trim();

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(furigana) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(phone) ||
            string.IsNullOrEmpty(address)) {
            await DisplayAlert("ƒGƒ‰[", "‚·‚×‚Ä‚Ì€–Ú‚ğ“ü—Í‚µ‚Ä‚­‚¾‚³‚¢B", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"///ConfirmPage?name={name}&furigana={furigana}&email={email}&phone={phone}&address={address}");
    }
}