using BenchmarkDotNet.Attributes;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using BenchmarkDotNet.Engines;
using Mapsui.Extensions;
using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Tests;
using Mapsui.Styles.Thematics;
using Mapsui.Nts.Providers;

#pragma warning disable IDISP001
#pragma warning disable IDISP003

namespace Mapsui.Rendering.Benchmarks;

[SimpleJob(RunStrategy.Throughput, iterationCount: 1, warmupCount: 0, invocationCount: 333, launchCount: 1)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class RenderToBitmapPerformance
{
    private static readonly RegressionMapControl skpMap;
    private static readonly RegressionMapControl pngMap;
    private static readonly RegressionMapControl webpMap;
    private static readonly RegressionMapControl map;
    private static readonly RegressionMapControl mapCached;
    private static readonly MapRenderer mapRenderer;
    private static readonly MapRenderer mapRendererCached;
    private static readonly MapRenderer mapRendererSkp;
    private static readonly MapRenderer mapRendererPng;
    private static readonly MapRenderer mapRendererWebp;

    static RenderToBitmapPerformance()
    {
        mapRenderer = new MapRenderer();
        mapRendererCached = new MapRenderer();
        mapRendererSkp = new MapRenderer();
        mapRendererPng = new MapRenderer();
        mapRendererWebp = new MapRenderer();
        skpMap = CreateMapControl(RenderFormat.Skp, mapRendererSkp);
        pngMap = CreateMapControl(RenderFormat.Png, mapRendererPng);
        webpMap = CreateMapControl(RenderFormat.WebP, mapRendererWebp);
        map = CreateMapControl(null, mapRenderer);
        mapCached = CreateMapControl(null, mapRendererCached);
        skpMap.WaitForLoadingAsync().Wait();
        pngMap.WaitForLoadingAsync().Wait();
        webpMap.WaitForLoadingAsync().Wait();
        map.WaitForLoadingAsync().Wait();
        mapCached.WaitForLoadingAsync().Wait();
        // render one time the map so that the sk path are cached.
        using var bitmap = mapRendererCached.RenderToBitmapStream(mapCached.Map.Navigator.Viewport, mapCached.Map!.Layers, Color.White);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Needs to be synchronous")]
    public static RegressionMapControl CreateMapControl(RenderFormat? renderFormat = null, MapRenderer? renderer = null)
    {
        var mapControl = new RegressionMapControl(renderer ?? mapRenderer);
        mapControl.SetSize(800, 600);

        mapControl.Map = CreateMap(renderFormat);

        // fetch data first time
        var fetchInfo = new FetchInfo(mapControl.Map.Navigator.Viewport.ToSection(), mapControl.Map.CRS);
        mapControl.Map.RefreshData(fetchInfo);
        mapControl.Map.Layers.WaitForLoadingAsync().Wait();

        return mapControl;
    }

    private static Map CreateMap(RenderFormat? renderFormat = null)
    {
        var map = new Map();

        var countrySource = new ShapeFile(GetAppDir() + $"{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}countries.shp", true);
        countrySource.CRS = "EPSG:4326";
        var projectedCountrySource = new ProjectingProvider(countrySource)
        {
            CRS = "EPSG:3857",
        };

        IProvider source = projectedCountrySource;

        if (renderFormat == RenderFormat.Skp)
        {
            source = new GeometrySimplifyProvider(projectedCountrySource);
            source = new GeometryIntersectionProvider(source);
        }

        ILayer layer = CreateCountryLayer(source);
        if (renderFormat != null)
        {
            var sqlitePersistentCache = new SqlitePersistentCache("Performance" + renderFormat);
            sqlitePersistentCache.Clear();
            layer = new RasterizingTileLayer(layer, mapRenderer, persistentCache: sqlitePersistentCache, renderFormat: renderFormat.Value);
        }

        map.Layers.Add(layer);

        var extent = map.Layers[0].Extent!;
        map.Home = n => n.ZoomToBox(extent);

        return map;
    }

    private static string GetAppDir()
    {
        var path = Path.GetDirectoryName(typeof(RenderToBitmapPerformance).Assembly.Location)!;
        return path;
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
        return new GradientTheme("POPDENS", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
    }

    [Benchmark]
    public void RenderDefault()
    {
        using var bitmap = mapRenderer.RenderToBitmapStream(map.Map.Navigator.Viewport, map.Map!.Layers, Color.White);
#if DEBUG
        File.WriteAllBytes(@$"{OutputFolder()}\Test.png", bitmap.ToArray());
#endif
    }

    [Benchmark]
    public void RenderDefaultCached()
    {
        using var bitmap = mapRendererCached.RenderToBitmapStream(mapCached.Map.Navigator.Viewport, mapCached.Map!.Layers, Color.White);
#if DEBUG
        File.WriteAllBytes(@$"{OutputFolder()}\Test.png", bitmap.ToArray());
#endif
    }

    [Benchmark]
    public void RenderRasterizingTilingPng()
    {
        using var bitmap = mapRendererPng.RenderToBitmapStream(pngMap.Map.Navigator.Viewport, pngMap.Map!.Layers, Color.White);
#if DEBUG
        File.WriteAllBytes(@$"{OutputFolder()}\Testpng.png", bitmap.ToArray());
#endif
    }

    [Benchmark]
    public void RenderRasterizingTilingWebP()
    {
        using var bitmap = mapRendererWebp.RenderToBitmapStream(webpMap.Map.Navigator.Viewport, webpMap.Map!.Layers, Color.White);
#if DEBUG
        File.WriteAllBytes(@$"{OutputFolder()}\Testwebp.png", bitmap.ToArray());
#endif
    }

    [Benchmark]
    public void RenderRasterizingTilingSkp()
    {
        using var bitmap = mapRendererSkp.RenderToBitmapStream(skpMap.Map.Navigator.Viewport, skpMap.Map!.Layers, Color.White);
#if DEBUG
        File.WriteAllBytes(@$"{OutputFolder()}\Testskp.png", bitmap.ToArray());
#endif
    }

#if DEBUG
    private string OutputFolder()
    {
        var path = Path.GetDirectoryName(typeof(RenderToBitmapPerformance).Assembly.Location);
        return path!;
    }
#endif
}
