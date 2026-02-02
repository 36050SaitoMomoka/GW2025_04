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

        var stock = new LocalDisasterPreventionInformationApp.Models.Stock {
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
}