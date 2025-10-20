using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets.InfoWidgets;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.MapInfo;

public class GeoJsonInfoSample : ISample
{
    static GeoJsonInfoSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "GeoJsonMapInfo";
    public string Category => "MapInfo";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

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

        map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(dataSource)));
        map.Widgets.Add(new MapInfoWidget(map, l => l is RasterizingTileLayer));

        return map;
    }

    private static Layer CreateCityLabelLayer(IProvider citiesProvider) => new("City labels")
    {
        DataSource = citiesProvider,
        Enabled = true,
        Style = CreateCityLabelStyle(),
    };

    private static LabelStyle CreateCityLabelStyle()
        => new()
        {
            ForeColor = Color.Black,
            BackColor = new Brush(new Color(242, 239, 233)),
            LabelColumn = "city",
        };
}
