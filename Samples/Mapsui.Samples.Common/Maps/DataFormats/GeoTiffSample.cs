using Mapsui.Extensions.Provider;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoTiffSample : ISample
{
    static GeoTiffSample()
    {
        GeoTiffDeployer.CopyEmbeddedResourceToFile("example.shp");
    }

    public string Name => "GeoTiff";
    public string Category => "DataFormats";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        var examplePath = Path.Combine(GeoTiffDeployer.GeoTiffLocation, "example.tif");
        var gif = new GeoTiffProvider(examplePath, new List<Color> { Color.Red });
        map.Layers.Add(CreateGifLayer(gif));

        return map;
    }

    private static ILayer CreateGifLayer(IProvider gifSource)
    {
        return new Layer
        {
            Name = "GeoGif",
            DataSource = gifSource,
            Style = null,
        };
    }
}
