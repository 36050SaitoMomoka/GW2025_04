using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace LocalDisasterPreventionInformationApp.Models {
    public class Product {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }  //商品ID

        [NotNull]
        public string Name { get; set; }  //商品名
    }
}
