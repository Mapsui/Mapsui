using System.IO;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.UI;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Common.Maps.Styles;

public class ThemeStyleSample : IMapControlSample
{
    static ThemeStyleSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
    }

    public string Name => "ThemeStyle on shapefile";
    public string Category => "Styles";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        using var countrySource = new ShapeFile(countriesPath, true) { CRS = "EPSG:3785" };

        map.Layers.Add(CreateCountryLayer(countrySource));
        map.Layers.Add(CreateCityHoverPoints());

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
                case "brazil": //If country name is Denmark, fill it with green
                    style.Fill = new Brush(Color.Green);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "united states": //If country name is USA, fill it with Blue and add a red outline
                    style.Fill = new Brush(Color.Violet);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "china": //If country name is China, fill it with red
                    style.Fill = new Brush(Color.Orange);
                    style.Outline = new Pen(Color.Black);
                    break;
                case "japan": //If country name is China, fill it with red
                    style.Fill = new Brush(Color.Cyan);
                    style.Outline = new Pen(Color.Black);
                    break;
                default:
                    style.Fill = new Brush(Color.Gray);
                    style.Outline = new Pen(Color.FromArgb(0, 64, 64, 64));
                    break;
            }
            style.Outline = new Pen(Color.FromArgb(0, 64, 64, 64));
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

    private static SymbolStyle CreateCityStyle()
    {
        var location = typeof(GeodanOfficesLayerBuilder).LoadBitmapId("Images.location.png");

        return new SymbolStyle
        {
            BitmapId = location,
            SymbolOffset = new Offset { Y = 64 },
            SymbolScale = 0.25
        };
    }
}
