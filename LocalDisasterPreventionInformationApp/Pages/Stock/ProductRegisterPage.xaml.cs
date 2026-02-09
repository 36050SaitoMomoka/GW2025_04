using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

//ContentPageを継承
public partial class ProductRegisterPage : ContentPage {

    private readonly AppDatabase _db;

    public ProductRegisterPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        //PageTitleを「備蓄管理」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "備蓄管理";
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {

        string name = ProductNameEntry.Text?.Trim();
        string expire = ExpirationEntry.Text?.Trim();
        string quantity = QuantityEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(expire) ||
            string.IsNullOrEmpty(quantity) ||
            string.IsNullOrEmpty(category)) {
            await DisplayAlert("エラー", "すべての項目を入力してください。", "OK");
            return;
        }

        // 正規化処理
        string expireNormalized = NormalizeDate(expire);
        if (expireNormalized == null) {
            await DisplayAlert("エラー", "日付の形式が正しくありません。", "OK");
            return;
        }

        //日付チェック(存在しない日付はエラー)
        DateTime exp;
        if (!DateTime.TryParseExact(
                expire,
                "yyyyMMdd",
                null,
                System.Globalization.DateTimeStyles.None,
                out exp)) {
            await DisplayAlert("エラー","日付が正しくありません","OK");
            return;
        }

        //商品登録
        Product product;

        try {
            product = await _db.AddProductIfNotExistsAsync(name, category);
        }
        catch (InvalidOperationException ex) {
            await DisplayAlert("エラー", ex.Message, "OK");
            return;
        }

        var qty = int.Parse(QuantityEntry.Text);

        var stock = new Models.Stock {
            ProductId = product.ProductId,
            ExpirationDate = exp,
            Quantity = qty,
        };

        await _db.AddOrUpdateStockAsync(stock);

        await Shell.Current.GoToAsync("..");        //１つ前のページに戻る
    }

    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//StockPage");
    }

    // 消費期限正規化
    private string NormalizeDate(string input) {
        // 全角→半角
        input = input.Normalize(System.Text.NormalizationForm.FormKC);
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // 数字以外をすべて除去（例: 2024/02/01 → 20240201）
        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.Length != 8)
            return null;

        // yyyy/MM/dd に整形
        return $"{digits.Substring(0, 4)}/{digits.Substring(4, 2)}/{digits.Substring(6, 2)}";
    }

    // 消費期限自動フォーマット
    private bool _isEditing = false;

    private void OnExpirationChanged(object sender, TextChangedEventArgs e) {
        if (_isEditing) return;

        _isEditing = true;

        var entry = (Entry)sender;
        string text = entry.Text;

        if (string.IsNullOrEmpty(text)) {
            _isEditing = false;
            return;
        }

        // 数字だけ取り出す
        string digits = new string(text.Where(char.IsDigit).ToArray());

        if (digits.Length > 8)
            digits = digits.Substring(0, 8);

        // yyyy/MM/dd の形に整形
        if (digits.Length >= 5)
            text = $"{digits.Substring(0, 4)}/{digits.Substring(4)}";
        else if (digits.Length >= 4)
            text = $"{digits.Substring(0, 4)}/";
        else
            text = digits;

        if (digits.Length >= 7)
            text = $"{digits.Substring(0, 4)}/{digits.Substring(4, 2)}/{digits.Substring(6)}";

        entry.Text = text;

        _isEditing = false;
    }

    // 数量→数字以外入力させない
    private bool _isEditingQty = false;

    private void OnQuantityChanged(object sender, TextChangedEventArgs e) {
        if (_isEditingQty) return;

        _isEditingQty = true;

        var entry = (Entry)sender;

        // 数字だけ残す
        string digits = new string(entry.Text.Where(char.IsDigit).ToArray());

        // 先頭の 0 を防ぐ（必要なら）
        if (digits.StartsWith("0") && digits.Length > 1)
            digits = digits.TrimStart('0');

        entry.Text = digits;

        _isEditingQty = false;
    }
}