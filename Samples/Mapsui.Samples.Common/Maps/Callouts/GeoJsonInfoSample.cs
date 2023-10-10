using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Callouts;

public class GeoJsonInfoSample : ISample
{
    private static TextBox? _textBox;

    static GeoJsonInfoSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "3 GeoJson Info";
    public string Category => "Info";

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
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(dataSource))
        {
            IsMapInfoLayer = true,
        });

        map.Info += MapOnInfo;
        _textBox = new TextBox()
        {
            MarginY = 0,
            MarginX = 5,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            BackColor = new Color(255, 255,255, 125),
            Text = "Information"
        };
        map.Widgets.Add(_textBox);


        return map;
    }
    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        if (_textBox != null)
        {
            _textBox.Text = e.MapInfo?.Feature?.ToDisplayText();
        }
    }

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
        => new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle(),
            IsMapInfoLayer = true,
        };

    private static LabelStyle CreateCityLabelStyle()
        => new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            LabelColumn = "city",
        };
}
