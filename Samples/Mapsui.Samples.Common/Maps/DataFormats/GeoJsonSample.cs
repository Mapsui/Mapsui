using Mapsui.Extensions.Provider;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.UI;
using System.Collections.Generic;
using System.IO;
using Mapsui.Nts.Providers;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoJsonSample : IMapControlSample
{
    static GeoJsonSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "13 GeoJson";
    public string Category => "Data Formats";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();
        var examplePath = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var geoJson = new GeoJsonProvider(examplePath);
        map.Layers.Add(CreateGeoJsonLayer(geoJson));

        return map;
    }

    private static ILayer CreateGeoJsonLayer(IProvider geoJsonSource)
    {
        return new Layer
        {
            Name = "GeoJson",
            DataSource = geoJsonSource,
        };
    }
}
