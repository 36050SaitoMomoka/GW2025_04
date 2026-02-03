using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models {

    //在庫テーブル
    public class Stock {

        [PrimaryKey,AutoIncrement]
        public int StockId { get; set; }  //在庫ID

        [NotNull]
        public int ProductId { get; set; }  //商品ID

        public DateTime ExpirationDate { get; set; }  //消費期限

        [NotNull]
        public int Quantity { get; set; }  //数量
    }

    //StockPage画面表示用
    public class StockItemViewModel : INotifyPropertyChanged {
        public int StockId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        private int _quantity;
        public int Quantity {
            get => _quantity;
            set {
                if (_quantity != value) {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public string Category { get; set; }
        public string ExpireDate { get; set; }
        public DateTime ExpirationDateRaw { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
