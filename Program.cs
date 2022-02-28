using Newtonsoft.Json;

Console.WriteLine("load iran_countries.json file...");
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
    Console.WriteLine("iran_countries.json is empty");
    return;
}
Console.WriteLine("countries count:" + list_state.Count);

list_state = list_state.DistinctBy(p => p.state).ToList();
list_countries = list_state.GroupBy(p => p.province).Select(each => new Country
{
    province_en = each.FirstOrDefault()?.province_en,
    name = each.Key,
    state = each.ToList()
}).ToList();


Console.WriteLine("load geojson.json file...");

//deserialize
using (var sReader = new StreamReader(@"./geojson.json"))
using (var jReader = new JsonTextReader(sReader))
{
    list_gojson = serializer.Deserialize<List<GeoJson>>(jReader);
}
if (list_gojson.Count == 0)
{
    Console.WriteLine("geojson.json is empty");
    return;
}

//write files
Console.WriteLine("create output");
System.IO.Directory.CreateDirectory(@"./output/geojson");
var finded_geojson = new List<string>();

string normalizeName(string str)
{
    return str.Replace("آ", "ا").Replace("ئ","ی").Replace(" ", "");
}
foreach (var each_country in list_countries)
{

    //find releated countries
    var geojson_country = list_gojson
        .Where(p =>
            each_country.state
                .Exists(each =>
                    p.properties.name != null &&
                    normalizeName(p.properties.name).Contains(normalizeName(each.state))
                )
        ).ToList();

    finded_geojson.AddRange(geojson_country.Select(p => p.properties.name).ToList());
    Console.WriteLine($"create ./output/geojson/{each_country.province_en}.json file");

    using (var sw = new StreamWriter($@"./output/geojson/{each_country.province_en}.json"))
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

question:

Console.WriteLine($"Do you want to create scripts og geojson data for each file? (y/n)");
var inputstr = Console.ReadLine();

switch (inputstr)
{
    case "n":
        break;

    case "y":
        System.IO.Directory.CreateDirectory(@"./output/scripts");

        foreach (var each_country in list_countries)
        {
            Console.WriteLine($"create ./output/scripts/{each_country.province_en}.json file");
            var file_content = string.Empty;
            using (var sReader = new StreamReader($@"./output/geojson/{each_country.province_en}.json"))
            {
                file_content = sReader.ReadToEnd();
            }

            using (var sw = new StreamWriter($@"./output/scripts/{each_country.province_en}.js"))
            {
                file_content = $@"window.am5geodata_{each_country.province_en} = (function () {{ var map = {file_content}; return map; }})();";

                sw.Write(file_content);
            }
        }
        break;

    default:
        goto question;
}



Console.WriteLine("summary of converts");
Console.WriteLine("====================");
var lost_geojson = list_gojson.Where(p => !finded_geojson.Exists(each => each == p.properties.name)).ToList();
Console.WriteLine("finded_geojson.Count: " + finded_geojson.Count);
Console.WriteLine("lost_geojson.Count: " + lost_geojson.Count);
