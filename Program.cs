using System.Linq;
using Newtonsoft.Json;

Console.WriteLine("read countries json");
var serializer = new JsonSerializer();
var list_state = new List<State>();
var list_countries = new List<Country>();
var list_gojson = new List<GeoJson>();

//deserialize
using (var sReader = new StreamReader(@"./iran_countries.json"))
using (var jReader = new JsonTextReader(sReader))
{
    list_state = serializer.Deserialize<List<State>>(jReader);
}
if (list_state.Count == 0)
{
    Console.WriteLine("json is empty");
    return;
}
Console.WriteLine("list_countries.Count:" + list_state.Count);

list_state = list_state.DistinctBy(p => p.state).ToList();
list_countries = list_state.GroupBy(p => p.province).Select(each => new Country
{
    name = each.Key,
    state = each.ToList()
}).ToList();


Console.WriteLine("read geojson");

//deserialize
using (var sReader = new StreamReader(@"./geojson.json"))
using (var jReader = new JsonTextReader(sReader))
{
    list_gojson = serializer.Deserialize<List<GeoJson>>(jReader);
}
if (list_gojson.Count == 0)
{
    Console.WriteLine("json is empty");
    return;
}
System.IO.Directory.CreateDirectory(@"./output");
var finded_geojson = new List<string>();
foreach (var each_country in list_countries)
{

    //find releated countries
    var geojson_country = list_gojson
        .Where(p =>
            each_country.state
                .Exists(each =>
                    p.properties.name != null &&
                    p.properties.name.Contains(each.state)
                )
        ).ToList();

    finded_geojson.AddRange(geojson_country.Select(p => p.properties.name).ToList());

    using (var sw = new StreamWriter($@"./output/{each_country.name}.json"))
    using (JsonWriter writer = new JsonTextWriter(sw))
    {
        var geojsonToWrite = new
        {
            type = "FeatureCollection",
            features = geojson_country
        };
        serializer.Serialize(writer, geojsonToWrite);
    }
}
var lost_geojson = list_gojson.Where(p => !finded_geojson.Exists(each => each == p.properties.name)).ToList();
Console.WriteLine("finded_geojson.Count: " + finded_geojson.Count);
Console.WriteLine("lost_geojson.Count: " + lost_geojson.Count);
