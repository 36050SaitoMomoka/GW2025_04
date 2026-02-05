using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

public partial class RegisterPage : ContentPage {
    public RegisterPage() {
        InitializeComponent();
    }

    // 郵便番号入力時にAPIを呼ぶ
    private async void onZipChanged(object sender, TextChangedEventArgs e) {
        zipEntry.Text = zipEntry.Text.Normalize(NormalizationForm.FormKC);
        string zip = zipEntry.Text?.Trim();

        if (zip?.Length == 7)
            await FetchAddressFromZip(zip);
    }

    // zipcloud APIで住所取得
    private async Task FetchAddressFromZip(string zip) {
        try {
            string url = $"https://zipcloud.ibsnet.co.jp/api/search?zipcode={zip}";
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<ZipCloudResponse>(json);

            if (result?.results != null && result.results.Length > 0) {
                var r = result.results[0];

                prefEntry.Text = r.address1;
                cityEntry.Text = r.address2;
                townEntry.Text = r.address3;
            } else {
                prefEntry.Text = "";
                cityEntry.Text = "";
                townEntry.Text = "";
            }
        }
        catch {
            prefEntry.Text = "";
            cityEntry.Text = "";
            townEntry.Text = "";
        }
    }

    // 全角→半角、空白除去
    private string Normalize(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        input = input.Normalize(NormalizationForm.FormKC);
        input = input.Trim();
        input = Regex.Replace(input, @"\s+", " ");

        return input;
    }

    // 分割住所を1行に結合
    private string BuildFullAddress() {
        string addressLine = NormalizeAddressLine(addressLineEntry.Text);

        var parts = new List<string>
        {
        Normalize(prefEntry.Text),
        Normalize(cityEntry.Text),
        Normalize(townEntry.Text),
        addressLine,
        Normalize(buildingEntry.Text)
    };

        return string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    // 入力チェック（エラーラベル制御）
    private bool Validate() {
        bool ok = true;

        // 氏名
        if (string.IsNullOrWhiteSpace(nameEntry.Text)) {
            nameError.Text = "未入力です";
            nameError.IsVisible = true;
            ok = false;
        } else nameError.IsVisible = false;

        // フリガナ
        if (string.IsNullOrWhiteSpace(furiganaEntry.Text)) {
            furiganaError.Text = "未入力です";
            furiganaError.IsVisible = true;
            ok = false;
        } else furiganaError.IsVisible = false;

        // 郵便番号
        if (string.IsNullOrWhiteSpace(zipEntry.Text)) {
            zipError.Text = "未入力です";
            zipError.IsVisible = true;
            ok = false;
        } else zipError.IsVisible = false;

        // 町域
        if (string.IsNullOrWhiteSpace(townEntry.Text)) {
            townError.Text = "未入力です";
            townError.IsVisible = true;
            ok = false;
        } else townError.IsVisible = false;

        // 番地
        if (string.IsNullOrWhiteSpace(addressLineEntry.Text)) {
            addressError.Text = "未入力です";
            addressError.IsVisible = true;
            ok = false;
        } else addressError.IsVisible = false;

        return ok;
    }

    // 次へボタン
    private async void OnNextClicked(object sender, EventArgs e) {
        if (!Validate())
            return;

        string name = Normalize(nameEntry.Text);
        string furigana = Normalize(furiganaEntry.Text);
        string email = Normalize(emailEntry.Text);
        string phone = Normalize(phoneEntry.Text);

        string fullAddress = BuildFullAddress();

        await Shell.Current.GoToAsync(
            $"//ConfirmPage?name={name}&furigana={furigana}&email={email}&phone={phone}&address={fullAddress}");
    }

    // 住所正規化
    private string NormalizeAddressLine(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        input = input.Normalize(NormalizationForm.FormKC);
        input = input.Trim();

        // 「丁目」「番地」「号」「の」などをハイフンに統一
        input = input.Replace("丁目", "-")
                     .Replace("番地", "-")
                     .Replace("号", "")
                     .Replace("の", "-")
                     .Replace("?", "-")   // 全角ハイフン
                     .Replace("ー", "-")   // 長音をハイフン扱い
                     .Replace("―", "-")
                     .Replace("‐", "-");

        // 連続ハイフンを1つにまとめる
        input = Regex.Replace(input, "-+", "-");

        // 前後のハイフンを除去
        input = input.Trim('-');

        return input;
    }
}

// zipcloud API レスポンスモデル
public class ZipCloudResponse {
    public ZipResult[] results { get; set; }
}

public class ZipResult {
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string address3 { get; set; }
}