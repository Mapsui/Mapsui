using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using System.IO;

namespace Mapsui.Samples.Common.Maps.Performance;

public class HugeShapeFileWithSimplifierAndClippingSample : IMapControlSample
{
    private static TileLayer TileLayer = OpenStreetMap.CreateTileLayer();
    private static ILayer ShapeLayer1 = CreateShapeLayer("EZG_KB_LM.shp", "cache1");
    private static ILayer ShapeLayer2 = CreateShapeLayer("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp", "cache2");

    private IMapControl? _mapControl;
    private readonly Mapsui.Utilities.Performance _performance = new(10);

    public HugeShapeFileWithSimplifierAndClippingSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("EZG_KB_LM.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp");
    }

    public string Name => "Huge Shape File With Simplifier and Clipping";
    public string Category => "Performance";


    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(TileLayer);
        map.Layers.Add(ShapeLayer1);
        map.Layers.Add(ShapeLayer2);

        return map;
    }

    public void Setup(IMapControl mapControl)
    {
        _mapControl = mapControl;
        mapControl.Map = CreateMap();
        var widget = CreatePerformanceWidget();
        mapControl.Map.Widgets.Add(widget);
        mapControl.Performance = _performance;
        MapRenderer.RegisterWidgetRenderer(typeof(PerformanceWidget), new PerformanceWidgetRenderer());
    }

    private PerformanceWidget CreatePerformanceWidget() => new(_performance)
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new MRect(10),
        TextSize = 12,
        TextColor = Color.Black,
        BackColor = Color.White,
        Tapped = (s, e) =>
        {
            _mapControl?.Performance?.Clear();
            _mapControl?.RefreshGraphics();
            return true;
        }
    };

    private static ILayer CreateShapeLayer(string shapeName, string cacheName)
    {
        using var shapeFile = new Mapsui.Nts.Providers.Shapefile.ShapeFile(
           Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, shapeName), false)
        { CRS = "EPSG:3857" };

        //option 1: with clipping
        var provider = new GeometrySimplifyAndClippingProvider(shapeFile);

        //option 2: without clipping
        //var provider = new GeometrySimplifyProvider(shapeFile);

        //var sqlitePersistentCache = new SqlitePersistentCache(cacheName);
        //sqlitePersistentCache.Clear();

        using var layer = new Layer
        {
            Name = shapeName,
            DataSource = provider,
            Style = CreateVectorThemeStyle(),
        };

        //return layer;
        return new RasterizingLayer(layer);
        //return new RasterizingTileLayer(layer) { Enabled = false }; //really slow
        //return new RasterizingTileLayer(layer, persistentCache: sqlitePersistentCache) { Enabled = false }; //really slow
    }

    private static IThemeStyle CreateVectorThemeStyle()
    {
        var style = new VectorStyle()
        {
            Fill = new Brush(Color.Transparent),
            Line = new Pen
            {
                Color = Color.Black,
                Width = 2
            },
            //Opacity = vectorTheme.Opacity,
            Outline = new Pen
            {
                Color = Color.Black,
                Width = 2
            }
        };

        return new ThemeStyle(f =>
        {
            return style;
        });
    }


}
