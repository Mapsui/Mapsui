using System.IO;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Performance;

public class GeometrySimplifySample : IMapControlSample
{
    static GeometrySimplifySample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
    }

    public string Name => "Simplify Geometry";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        var countrySource = new ShapeFile(countriesPath, true)
        {
            CRS = "EPSG:4326"
        };
        var projectedCountrySource = new ProjectingProvider(countrySource)
        {
            CRS = "EPSG:3857",
        };

        var simplifyCountrySource = new GeometrySimplifyProvider(projectedCountrySource, distanceTolerance: 200000);
        map.Layers.Add(new RasterizingLayer(CreateCountryLayer(simplifyCountrySource)));

        return map;
    }

    private static ILayer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateCountryTheme()
        };
    }

    private static IThemeStyle CreateCountryTheme()
    {
        // Set a gradient theme on the countries layer, based on Population density
        // First create two styles that specify min and max styles
        // In this case we will just use the default values and override the fill-colors
        // using a color blender. If different line-widths, line- and fill-colors where used
        // in the min and max styles, these would automatically get linearly interpolated.
        var min = new VectorStyle { Outline = new Pen { Color = Color.Black } };
        var max = new VectorStyle { Outline = new Pen { Color = Color.Black } };

        // Create theme using a density from 0 (min) to 400 (max)
        return new GradientTheme("PopDens", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
    }
}
