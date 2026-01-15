namespace LocalDisasterPreventionInformationApp.Models;

public class YahooRouteResponse {
    public List<Feature> Feature { get; set; }
}

public class Feature {
    public Geometry Geometry { get; set; }
}

public class Geometry {
    public List<List<double>> Coordinates { get; set; }
}