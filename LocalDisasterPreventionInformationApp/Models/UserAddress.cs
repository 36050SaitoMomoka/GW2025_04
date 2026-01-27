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
        public string Address { get; set; }  //住所

        [NotNull]
        public double Longitude { get; set; }  //経度

        [NotNull]
        public double Latitude { get; set; }  //緯度
    }
}
