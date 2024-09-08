using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class ThemeStyleSample : ISample
{
    static ThemeStyleSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
    }

    public string Name => "ThemeStyle on shapefile";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        using var countrySource = new ShapeFile(countriesPath, true) { CRS = "EPSG:3785" };

        map.Layers.Add(CreateCountryLayer(countrySource));
        map.Layers.Add(CreateCityHoverPoints());

        map.Widgets.Add(new MapInfoWidget(map));

        return map;
    }

    private static ILayer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateThemeStyle(),
            IsMapInfoLayer = true
        };
    }

    private static ThemeStyle CreateThemeStyle()
    {
        return new ThemeStyle(f =>
        {
            if (f is GeometryFeature geometryFeature)
                if (geometryFeature.Geometry is Point)
                    return null;

            var style = new VectorStyle();

            switch (f["NAME"]?.ToString()?.ToLower())
            {
                case "brazil": //If country name is Brazil, fill it with green
                    style.Fill = new Brush(Color.Green);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "united states": //If country name is USA, fill it with violet
                    style.Fill = new Brush(Color.Violet);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "china": //If country name is China, fill it with orange
                    style.Fill = new Brush(Color.Orange);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "japan": //If country name is Japan, fill it with cyan
                    style.Fill = new Brush(Color.Cyan);
                    style.Outline = new Pen(Color.Black);
                    break;
                default:
                    style.Fill = new Brush(Color.Gray);
                    style.Outline = new Pen(Color.FromArgb(0, 64, 64, 64));
                    break;
            }
            return style;
        });
    }

    public static ILayer CreateCityHoverPoints()
    {
        var features = WorldCitiesFeaturesBuilder.CreateTop100Cities();

        return new MemoryLayer
        {
            Features = features,
            Style = CreateCityStyle(),
            Name = "Points",
            IsMapInfoLayer = true
        };
    }

    private static SymbolStyle CreateCityStyle() => new()
    {
        ImageSource = "embedded://Mapsui.Samples.Common.Images.location.png",
        SymbolOffset = new Offset { Y = 64 },
        SymbolScale = 0.25
    };
}
