public class GeoJson
{
    public string type { get; set; }
    public List<GeoJsonFeatures> features { get; set; }
}

public class GeoJsonFeatures
{
    public string type { get; set; }
    public object geometry { get; set; }
    public GeoProperties properties { get; set; }
}

public class GeoProperties
{
    //id
    public string ADM2_PCODE { get; set; }

    //city name en
    public string ADM2_EN { get; set; }
    //city name fa
    public string ADM2_FA { get; set; }

    //province name en
    public string ADM1_EN { get; set; }
    //province name fa
    public string ADM1_FA { get; set; }
    
}