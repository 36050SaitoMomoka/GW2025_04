using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

//ContentPageÇåpè≥
public partial class ProductRegisterPage : ContentPage {

    private readonly AppDatabase _db;

    public ProductRegisterPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        //PageTitleÇÅuîıí~ä«óùÅvÇ…Ç∑ÇÈ
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "îıí~ä«óù";
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {

        //è§ïiìoò^
        await _db.AddProductIfNotExistsAsync(ProductNameEntry.Text);

        //ç›å…
        var product = (await _db.GetProductsAsync())
            .First(p => p.Name == ProductNameEntry.Text);

        //var stock = new Stock {
        //    ProductId = product.ProductId,
        //    ExpirationDate = DateTime.Parse(ExpirationEntry.Text),
        //    Quantity = int.Parse(QuantityEntry.Text),
        //};

        //await _db.AddOrUpdateStockAsync(stock);

        await Shell.Current.GoToAsync("..");        //ÇPÇ¬ëOÇÃÉyÅ[ÉWÇ…ñﬂÇÈ
    }
}