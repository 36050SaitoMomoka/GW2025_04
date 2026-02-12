using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models {

    //ユーザ住所テーブル
    public class UserAddress {

        [PrimaryKey, AutoIncrement]
        public int AddressId { get; set; }  //住所ID

        [NotNull]
        public string AddressType { get; set; }  //住所種類

        [NotNull]
        public string PostalCode { get; set; }  //郵便番号

        [NotNull]
        public string Address { get; set; }  //住所

        public double? Longitude { get; set; }  //経度

        public double? Latitude { get; set; }  //緯度
    }
}
