using System.Diagnostics.CodeAnalysis;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.UI;
using System.IO;
using Mapsui.Extensions;
using Mapsui.Nts.Providers;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;

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

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don\'t ignore created IDisposable")]
    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857", // The Map CRS needs to be set   
        };

        var examplePath = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var geoJson = new GeoJsonProvider(examplePath)
        {
            CRS = "EPSG:4326" // The DataSource CRS needs to be set
        };

        var dataSource = new ProjectingProvider(geoJson)
        {
            CRS = "EPSG:3857",
        };

        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(dataSource)));

        return map;
    }

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
    {
        return new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle()
        };
    }

    private static LabelStyle CreateCityLabelStyle()
    {
        return new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            LabelColumn = "city",
        };
    }
}
