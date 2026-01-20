using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<double> coordinates { get; set; }
    }

    //避難所の属性（名前・住所など）
    public class ShelterProperties {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
    }
}
