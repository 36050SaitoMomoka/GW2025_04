namespace LocalDisasterPreventionInformationApp.Models;

public class ShelterResponse {
    public string name { get; set; }
    public Location location { get; set; }
}

public class Location {
    public double lat { get; set; }
    public double lon { get; set; }
}