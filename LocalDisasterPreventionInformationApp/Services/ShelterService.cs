using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Models;
using LocalDisasterPreventionInformationApp.Models.GeoJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Services {
    public class ShelterService {
        private readonly HttpClient _http;
        private readonly AppDatabase _db;

        public ShelterService(HttpClient http, AppDatabase db) {
            _http = http;
            _db = db;
        }

        //GeoJSONを読み込んでDBに保存
        public async Task<int> FetchAndSaveShelterAsync() {

            string json = null;

            // Resources/Rawに置いたGeoJSONを読み込む
#if ANDROID
            //Android向け
            using var stream = Android.App.Application.Context.Assets.Open("mergeFromCity_2.geojson");
            using var reader = new StreamReader(stream);
            json = reader.ReadToEnd();
#endif

#if WINDOWS
            //Windows向け
            using var stream = await FileSystem.OpenAppPackageFileAsync("mergeFromCity_2.geojson");
            using var reader = new StreamReader(stream);
            json = await reader.ReadToEndAsync();
#endif

            //GeoJSONをC#モデルに変換
            var root = JsonSerializer.Deserialize<ShelterGeoJsonRoot>(json);

            if (root?.features == null)
                return 0;

            int count = 0;

            foreach (var f in root.features) {

                var address = f.properties.Address ?? "";

                var s = new Shelter {
                    ShelterId = f.properties.CommonID,
                    Name = f.properties.FacilityName,
                    Address = f.properties.Address,
                    Prefecture = ExtractPrefecture(f.properties.PrefAndCity),
                    City = ExtractCity(f.properties.PrefAndCity),
                    Longitude = f.geometry.coordinates[0],
                    Latitude = f.geometry.coordinates[1],
                };

                await _db.SaveShelterAsync(s);
                count++;
            }

            return count;
        }

        //都道府県を抽出
        private string ExtractPrefecture(string address) {

            if (string.IsNullOrWhiteSpace(address))
                return "";

            string[] prefs = {
                "北海道","青森県","岩手県","宮城県","秋田県","山形県","福島県",
                "茨城県","栃木県","群馬県","埼玉県","千葉県","東京都","神奈川県",
                "新潟県","富山県","石川県","福井県","山梨県","長野県",
                "岐阜県","静岡県","愛知県","三重県",
                "滋賀県","京都府","大阪府","兵庫県","奈良県","和歌山県",
                "鳥取県","島根県","岡山県","広島県","山口県",
                "徳島県","香川県","愛媛県","高知県",
                "福岡県","佐賀県","長崎県","熊本県","大分県","宮崎県","鹿児島県","沖縄県"
            };

            return prefs.FirstOrDefault(p => address.StartsWith(p)) ?? "";
        }

        //市区町村を抽出
        private string ExtractCity(string address) {

            if (string.IsNullOrWhiteSpace(address))
                return "";

            var pref = ExtractPrefecture(address);
            var rest = address.Substring(pref.Length).Trim();

            // 郡 → 町 のパターン
            // 郡 → 村 のパターン
            var gunIdx = rest.IndexOf("郡");
                if (gunIdx >= 0) {
                    var townIdx = rest.IndexOf("町", gunIdx);
                    var villageIdx = rest.IndexOf("村", gunIdx);

                    int endIdx = new[] { townIdx, villageIdx }.Where(i => i >= 0).DefaultIfEmpty(-1).Min();

                    if (endIdx >= 0)
                      return rest.Substring(0, endIdx + 1);
                }

            // 市 / 区 / 町 / 村 のパターン
            var markers = new[] { "市", "区", "町", "村" };

            foreach (var m in markers) {
                var idx = rest.IndexOf(m);

                if (idx >= 0)
                    return rest.Substring(0, idx + 1);
            }

            return rest;

        }
    }
}
