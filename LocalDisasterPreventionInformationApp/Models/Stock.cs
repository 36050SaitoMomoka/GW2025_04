using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models {

    //在庫テーブル
    public class Stock {

        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }  //在庫ID

        [NotNull]
        public int ProductId { get; set; }  //商品ID

        [NotNull]
        public DateTime ExpirationDate { get; set; }  //消費期限

        [NotNull]
        public int Quantity { get; set; }  //数量
    }
}
