using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace LocalDisasterPreventionInformationApp.Pages.Register;

public partial class RegisterPage : ContentPage {
    public RegisterPage() {
        InitializeComponent();

        BindingContext = Shell.Current.BindingContext;
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

        string name = NormalizeName(nameEntry.Text);
        string furigana = NomalizeFurigana(furiganaEntry.Text);
        string email = NomalizeEmail(emailEntry.Text);
        string phone = NormalizePhone(phoneEntry.Text);

        string fullAddress = BuildFullAddress();

        await Shell.Current.GoToAsync(
            $"//ConfirmPage?name={name}&furigana={furigana}&email={email}&phone={phone}&address={fullAddress}&postal={zipEntry.Text}");
    }

    // 氏名正規化
    private string NormalizeName(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // 全角→半角
        input = input.Normalize(NormalizationForm.FormKC);

        // スペース削除（半角・全角）
        input = input.Replace(" ", "").Replace("　", "");

        return input;
    }

    // フリガナ正規化
    private string NomalizeFurigana(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // スペース削除
        input = input.Replace(" ", "").Replace("　", "");

        var sb = new StringBuilder();

        foreach (char c in input) {
            // ひらがな → カタカナ（Unicode ひらがな全域）
            if (c >= '\u3041' && c <= '\u3096') {
                sb.Append((char)(c + 0x60)); // ひらがな→カタカナ
            } else {
                sb.Append(c);
            }
        }

        // 半角カナ → 全角カナ
        string result = sb.ToString().Normalize(NormalizationForm.FormKC);

        return result;
    }

    // メールアドレス正規化
    private string _lastValidEmail = "";

    private void OnEmailFilter(object sender, TextChangedEventArgs e) {
        var text = emailEntry.Text;

        if (string.IsNullOrEmpty(text)) {
            _lastValidEmail = "";
            return;
        }

        // 半角 ASCII のみ許可（0x21?0x7E）
        bool isValid = text.All(c =>
            c >= 0x21 && c <= 0x7E &&  // ASCII
            c != ' ' && c != '　'      // 半角/全角スペース禁止
        );

        if (!isValid) {
            // 入力前の状態に戻す（＝入力できないように見える）
            emailEntry.Text = _lastValidEmail;
            return;
        }

        _lastValidEmail = text;
    }

    private string NomalizeEmail(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // 全角 → 半角
        input = input.Normalize(NormalizationForm.FormKC);

        // 前後の空白除去
        input = input.Trim();

        // 全角スペース除去
        input = input.Replace("　", "");

        // 小文字に統一
        input = input.ToLowerInvariant();

        return input;
    }

    // 電話番号正規化
    private string _lastValidPhone = "";

    private void OnPhoneFilter(object sender, TextChangedEventArgs e) {
        var text = phoneEntry.Text;

        if (string.IsNullOrEmpty(text)) {
            _lastValidPhone = "";
            return;
        }

        // 数字のみ許可（全角数字も許可）
        bool isValid = text.All(c => char.IsDigit(c));

        if (!isValid) {
            phoneEntry.Text = _lastValidPhone;
            return;
        }

        _lastValidPhone = text;
    }

    string NormalizePhone(string input) {
        if (string.IsNullOrWhiteSpace(input)) return "";

        // 半角化
        string num = input.Normalize(NormalizationForm.FormKC);

        // 数字以外除去
        num = new string(num.Where(char.IsDigit).ToArray());

        // --- 優先判定（特殊番号） ---

        // 0120（フリーダイヤル）
        if (num.StartsWith("0120") && num.Length == 10)
            return $"{num[..4]}-{num.Substring(4, 3)}-{num.Substring(7)}";

        // 0570（ナビダイヤル）
        if (num.StartsWith("0570") && num.Length == 10)
            return $"{num[..4]}-{num.Substring(4, 2)}-{num.Substring(6)}";

        // --- 携帯電話（090/080/070/060） ---
        if ((num.StartsWith("090") || num.StartsWith("080") ||
             num.StartsWith("070") || num.StartsWith("060")) &&
             num.Length == 11)
            return $"{num[..3]}-{num.Substring(3, 4)}-{num.Substring(7)}";

        // --- IP電話（050） ---
        if (num.StartsWith("050") && num.Length == 11)
            return $"{num[..3]}-{num.Substring(3, 4)}-{num.Substring(7)}";

        // --- 固定電話（関東圏のみ対応） ---

        // 市外局番 4桁（例：0276 太田市）
        string[] area4 = { "0276", "0285", "0297", "0299", "0466", "0476", "0479" };
        if (num.Length == 10 && area4.Any(a => num.StartsWith(a)))
            return $"{num[..4]}-{num.Substring(4, 2)}-{num.Substring(6)}";

        // 市外局番 3桁（例：027, 028, 029, 03, 04, 045, 046, 047, 048, 049）
        string[] area3 = { "027", "028", "029", "03", "04", "045", "046", "047", "048", "049" };
        if (num.Length == 10 && area3.Any(a => num.StartsWith(a)))
            return $"{num[..3]}-{num.Substring(3, 3)}-{num.Substring(6)}";

        // 市外局番 2桁（03, 06）
        if (num.Length == 10 && (num.StartsWith("03") || num.StartsWith("06")))
            return $"{num[..2]}-{num.Substring(2, 4)}-{num.Substring(6)}";

        // --- それ以外は整形しない ---
        return num;
    }

    // 住所正規化
    private string NormalizeAddressLine(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // 全角 → 半角
        input = input.Normalize(NormalizationForm.FormKC);

        // スペース削除（半角・全角）
        input = input.Replace(" ", "").Replace("　", "");

        // 「丁目」「番地」「号」「の」などをハイフンに統一
        input = input.Replace("丁目", "-")
                     .Replace("番地", "-")
                     .Replace("号", "")
                     .Replace("の", "-");

        // 長音記号をハイフンに統一
        input = input.Replace("ー", "-")
                     .Replace("―", "-")
                     .Replace("?", "-")
                     .Replace("‐", "-");

        // 住所に不要な記号を削除
        input = Regex.Replace(input, @"[!@#$%^&*+=~|\\/?<>]", "");

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