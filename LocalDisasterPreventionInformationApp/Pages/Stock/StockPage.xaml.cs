using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

//ContentPageを継承
public partial class StockPage : ContentPage {

    private readonly AppDatabase _db;

    public ObservableCollection<object> Items { get; set; }
           = new ObservableCollection<object>();

    public ObservableCollection<object> ExpiringItems { get; set; }
           = new ObservableCollection<object>();

    public ICommand IncreaseCommand { get; }
    public ICommand DecreaseCommand { get; }

    public StockPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        IncreaseCommand = new Command<object>(IncreaseQuantity);
        DecreaseCommand = new Command<object>(DecreaseQuantity);

        Inner.BindingContext = this;
        LoadData();

        //PageTitleを「備蓄管理」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "備蓄管理";
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData() {
        var products = await _db.GetProductsAsync();
        var stocks = await _db.GetStocksAsync();
        Items.Clear();
        ExpiringItems.Clear();

        //期限が近い順に並べ替える
        var sortedStocks = stocks.OrderBy(s => s.ExpirationDate).ToList();

        foreach (var s in sortedStocks) {
            var p = products.First(x => x.ProductId == s.ProductId);

            Items.Add(new StockItemViewModel {
                StockId = s.StockId,
                ProductId = s.ProductId,
                ProductName = p.Name,
                Quantity = s.Quantity,
                ExpireDate = s.ExpirationDate.ToString("yyyy/MM/dd"),
                ExpirationDateRaw = s.ExpirationDate
            });

            var today = DateTime.Today;

            //期限切れ(今日より前)
            if (s.ExpirationDate < today) {
                ExpiringItems.Add(new {
                    Message = $"{p.Name}の期限が切れています({s.ExpirationDate:yyyy/MM/dd})"
                });
                //期限が近い商品
            } else if ((s.ExpirationDate - today).TotalDays <= 30) {
                ExpiringItems.Add(new {
                    Message = $"{p.Name}の期限が近いです({s.ExpirationDate:yyyy/MM/dd})"
                });
            }
        }
    }

    private async void IncreaseQuantity(object item) {
        var stock = item as StockItemViewModel;
        if (stock == null) return;

        stock.Quantity++;

        // DB更新（1個増やす）
        await _db.AddOrUpdateStockAsync(new Models.Stock {
            StockId = stock.StockId,
            ProductId = stock.ProductId,
            ExpirationDate = stock.ExpirationDateRaw,
            Quantity = 1
        });

        // 画面更新
        var index = Items.IndexOf(item);
        Items[index] = stock;
    }

    private async void DecreaseQuantity(object item) {
        var stock = item as StockItemViewModel;
        if (stock == null || stock.Quantity <= 0) return;

        stock.Quantity--;

        // DB更新（1個減らす）
        await _db.ReduceStockAsync(
            stock.ProductId,
            stock.ExpirationDateRaw,
            1
        );

        // 画面更新
        var index = Items.IndexOf(item);
        Items[index] = stock;
    }

    private async void OnRegisterClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("product");
    }
}