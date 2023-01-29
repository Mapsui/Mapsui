using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Mapsui.Nts.Providers;

public class GeoJsonProvider : IProvider
{
    private string _geoJson;
    
    public GeoJsonProvider(string geojson)
    {
        _geoJson = geojson;
    }

    public string? CRS { get; set; }
    public MRect? GetExtent()
    {
    }

    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(_geoJson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            geometry = serializer.Deserialize<Geometry>(jsonReader);
        }
    }
}
