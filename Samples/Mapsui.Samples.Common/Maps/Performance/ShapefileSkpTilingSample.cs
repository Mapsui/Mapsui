using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using System.IO;
using System.Reflection;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class ShapefileSkpTileSample : IMapControlSample
{

    public string Name => "Skia tiling on shapefile";

    static ShapefileSkpTileSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("cities.shp");
    }

    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();

        var countrySource = new ShapeFile(ShapeFilesDeployer.ShapeFilesLocation + "\\countries.shp", true)
        {
            CRS = "EPSG:4326"
        };
        var projectedCountrySource = new ProjectingProvider(countrySource)
        {
            CRS = "EPSG:3857",
        };

        var geometrySimplify = new GeometrySimplifyProvider(projectedCountrySource);
        var geometryIntersection = new GeometryIntersectionProvider(geometrySimplify);

        var sqlitePersistentCache = new SqlitePersistentCache("countriesSkp");
        sqlitePersistentCache.Clear();
        map.Layers.Add(new RasterizingTileLayer(CreateCountryLayer(geometryIntersection), persistentCache: sqlitePersistentCache, renderFormat: RenderFormat.Skp));

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
