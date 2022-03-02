using System.Text;
using Newtonsoft.Json;

Console.WriteLine("load iran_countries.json file...");
var serializer = new JsonSerializer();
var list_state = new List<State>();
var list_countries = new List<Country>();
var gojson = new GeoJson();
var list_gojson = new List<GeoJsonFeatures>();

string normalizeNameFa(string str)
{
    return str.Replace("آ", "ا").Replace("ئ", "ی").Replace(" ", "");
}

string normalizeNameEn(string str)
{
    return str.Replace(" ", "_").Replace("-", "");
}
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
using (var sReader = new StreamReader(@"./iran_city.json"))
using (var jReader = new JsonTextReader(sReader))
{
    gojson = serializer.Deserialize<GeoJson>(jReader);
    list_gojson = gojson.features;
}
if (list_gojson.Count == 0)
{
    Console.WriteLine("iran_city.json is empty");
    return;
}

//write files
Console.WriteLine("create output");
System.IO.Directory.CreateDirectory(@"./output/geojson");
var finded_geojson = new List<string>();

foreach (var each_country in list_countries)
{
    //find releated countries
    var geojson_country = list_gojson
        .Where(p =>
            each_country.state
                .Exists(each =>
                    p.properties.ADM1_EN != null &&
                    normalizeNameEn(p.properties.ADM1_EN).Contains(normalizeNameEn(each.province_en))
                )
        ).ToList();

    finded_geojson.AddRange(geojson_country.Select(p => p.properties.ADM1_EN).ToList());
    Console.WriteLine($"create ./output/geojson/{each_country.province_en}.json file");

    using (var sw = new StreamWriter($@"./output/geojson/{each_country.province_en}.json"))
    using (JsonWriter writer = new JsonTextWriter(sw))
    {
        var normalizeGeoJsonObject = geojson_country.Select(p => new
        {
            type = p.type,
            geometry = p.geometry,
            properties = new
            {
                province_en = p.properties.ADM1_EN,
                province_fa = p.properties.ADM1_FA,
                city_en = p.properties.ADM2_EN,
                city_fa = p.properties.ADM2_FA,
                name = p.properties.ADM2_FA,
                id = p.properties.ADM2_PCODE
            }
        });

        var geojsonToWrite = new
        {
            type = "FeatureCollection",
            features = normalizeGeoJsonObject
        };
        serializer.Serialize(writer, geojsonToWrite);
    }
}

Console.WriteLine($"Normalizing all name of province name");

string replaceValueByName(string line, string fieldName, Func<string, string> changeValueAction)
{

    var indexName_EN = line.LastIndexOf(fieldName);
    if (indexName_EN != -1)
    {
        var templine = line.Substring(indexName_EN + fieldName.Length + 2);
        var strName_EN = templine.Substring(0, templine.IndexOf(","));
        var newStrName_EN = changeValueAction(strName_EN);
        line = line.Replace(strName_EN, newStrName_EN);
    }
    return line;
}
using (var sReader = new StreamReader(@"./iranLow.js"))
{
    StringBuilder newContent = new StringBuilder();
    var line = string.Empty;
    var indexName_EN = 0;
    while ((line = sReader.ReadLine()) != null)
    {
        line = replaceValueByName(line, "NAME_ENG", (str) => str.Replace("Ä", "a").Replace("ā", "a").Replace("-", "").Trim().Replace(" ", "_"));
        var name_fa = list_countries.FirstOrDefault(p => line.Contains(p.province_en))?.name;
        if (name_fa != null)
        {
            line = replaceValueByName(line, "name", (str) => $"\"{name_fa}\"");
        }
        newContent.AppendLine(line);
    }
    sReader.Dispose();

    //overwrite
    using (var sw = new StreamWriter(@"./output/iranLow.js"))
        sw.Write(newContent);
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
var lost_geojson = list_gojson.Where(p => !finded_geojson.Exists(each => each == p.properties.ADM1_EN)).ToList();
Console.WriteLine("finded_geojson.Count: " + finded_geojson.Count);
Console.WriteLine("lost_geojson.Count: " + lost_geojson.Count);
