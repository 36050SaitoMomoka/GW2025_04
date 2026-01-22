using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Models.GeoJson {
    //GeoJSON全体のルート
    public class ShelterGeoJsonRoot {
        public List<ShelterFeature> features { get; set; }
    }

    //各避難所（Feature）
    public class ShelterFeature {
        public ShelterGeometry geometry { get; set; }
        public ShelterProperties properties { get; set; }
    }

    //座標データ
    public class ShelterGeometry {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }

    //避難所の属性（名前・住所など）
    public class ShelterProperties {
        [JsonPropertyName("共通ID")]
        public string CommonID { get; set; }

        [JsonPropertyName("都道府県名及び市町村名")]
        public string PrefAndCity { get; set; }

        [JsonPropertyName("施設・場所名")]
        public string FacilityName { get; set; }

        [JsonPropertyName("住所")]
        public string Address { get; set; }
    }
}
