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

    public string Name => "ThemeStyleOnShapefile";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        using var countrySource = new ShapeFile(countriesPath, true) { CRS = "EPSG:3785" };

        map.Layers.Add(CreateCountryLayer(countrySource));
        map.Layers.Add(CreateCityHoverPoints());

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Countries" || l.Name == "Points"));

        return map;
    }

    private static ILayer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateThemeStyle(),
        };
    }

    private static ThemeStyle CreateThemeStyle()
    {
        // Pre-create styles to enable caching. Each style instance has a stable GenerationId,
        // allowing the drawable cache to reuse cached drawables for features with the same style.
        // Creating new style instances on every call would defeat caching since each would have
        // a different GenerationId.
        var brazilStyle = new VectorStyle
        {
            Fill = new Brush(Color.Green),
            Outline = new Pen(Color.Black)
        };

        var usaStyle = new VectorStyle
        {
            Fill = new Brush(Color.Violet),
            Outline = new Pen(Color.Black)
        };

        var chinaStyle = new VectorStyle
        {
            Fill = new Brush(Color.Orange),
            Outline = new Pen(Color.Black)
        };

        var japanStyle = new VectorStyle
        {
            Fill = new Brush(Color.Cyan),
            Outline = new Pen(Color.Black)
        };

        var defaultStyle = new VectorStyle
        {
            Fill = new Brush(Color.Gray),
            Outline = new Pen(Color.FromArgb(0, 64, 64, 64))
        };

        return new ThemeStyle(f =>
        {
            if (f is GeometryFeature geometryFeature)
                if (geometryFeature.Geometry is Point)
                    return null;

            return f["NAME"]?.ToString()?.ToLower() switch
            {
                "brazil" => brazilStyle,
                "united states" => usaStyle,
                "china" => chinaStyle,
                "japan" => japanStyle,
                _ => defaultStyle
            };
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
        };
    }

    private static ImageStyle CreateCityStyle() => new()
    {
        Image = "embedded://Mapsui.Samples.Common.Images.location.png",
        Offset = new Offset { Y = 64 },
        SymbolScale = 0.25
    };
}
