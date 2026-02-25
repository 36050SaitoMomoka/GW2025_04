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

        // 翻訳用
        BindingContext = Shell.Current.BindingContext;

        //PageTitleを「備蓄管理」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = vm.Header_Stock;
            vm.PropertyChanged += (s, e) => {
                if (e.PropertyName == null || e.PropertyName == "SelectedLangage") {
                    SetPickerItems(vm);
                }
            };
            // 初回セット
            SetPickerItems(vm);

            vm.PageTitle = vm.Header_Stock;
        }
    }

    // Pickerの中身を翻訳
    private void SetPickerItems(AppShellViewModel vm) {
        CategoryPicker.ItemsSource = new List<string> {
            vm.Product_Food,
            vm.Product_Beverages,
            vm.Product_Consumables,
            vm.Product_Other
        };
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {

        string name = ProductNameEntry.Text?.Trim();
        DateTime expire = ExpirationPicker.Date;
        string quantity = QuantityEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(quantity) ||
            string.IsNullOrEmpty(category)) {
            await DisplayAlert("エラー", "すべての項目を入力してください。", "OK");
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
            ExpirationDate = expire,
            Quantity = qty,
        };

        await _db.AddOrUpdateStockAsync(stock);

        await Shell.Current.GoToAsync("..");        //１つ前のページに戻る
    }

    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//StockPage");
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