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