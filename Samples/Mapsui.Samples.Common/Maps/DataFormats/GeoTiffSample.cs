using Mapsui.Extensions.Provider;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.UI;
using System.Collections.Generic;
using System.IO;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoTiffSample : IMapControlSample
{
    static GeoTiffSample()
    {
        GeoTiffDeployer.CopyEmbeddedResourceToFile("example.shp");
    }

    public string Name => "10 Geo Tiff";
    public string Category => "Data Formats";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

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
        };
    }
}
