public class GeoJson
{
    public string type { get; set; }
    public object geometry { get; set; }
    public GeoProperties properties { get; set; }
}

public class GeoProperties{
    public string name { get; set; }
    public string province { get; set; }
}


// type":"Feature","properties":{"@id":"relation/269911","admin_level":"5","name":"شهرستان شیروان","place":"county","type":"boundary"},"geometry"