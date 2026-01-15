using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models {
    public class Shelter {
        [PrimaryKey]
        public string ShelterId { get; set; }  //避難所ID(APIのIDをそのまま)

        [NotNull]
        public string Name { get; set; }  //避難所名

        [NotNull]
        public string Address { get; set; }  //住所

        [NotNull]
        public string Prefecture { get; set; }  //都道府県

        [NotNull]
        public string City { get; set; }  //市区町村

        [NotNull]
        public double Longitude { get; set; }  //経度

        [NotNull]
        public double Latitude { get; set; }  //緯度
    }
}
