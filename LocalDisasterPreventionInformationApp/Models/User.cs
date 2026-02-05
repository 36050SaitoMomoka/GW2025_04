using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models {

    //ユーザ情報テーブル
    public class User {

        [PrimaryKey,AutoIncrement]
        public int UserId { get; set; }  //ユーザID

        [NotNull]
        public string Name { get; set; }  //氏名

        [NotNull]
        public string Furigana { get; set; }  //フリガナ

        //電話番号とメアドはNULL許容
        public string PhoneNumber { get; set; }  //電話番号

        public string Email { get; set; }  //メールアドレス
    }
}
