using System.IO;
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
using SkiaSharp;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Utilities;

#pragma warning disable IDISP001
#pragma warning disable IDISP003

namespace Mapsui.Rendering.Benchmarks;

[SimpleJob(RunStrategy.Throughput, targetCount: 1, warmupCount: 0, invocationCount: 333, launchCount: 1)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class RenderToCpuPerformance : IDisposable
{
    private static readonly RegressionMapControl tilingSkpMap;
    private static readonly RegressionMapControl tilingPngMap;
    private static readonly RegressionMapControl tilingWebpMap;
    private static readonly RegressionMapControl map;
    private static readonly RegressionMapControl rasterizingPngMap;
    private static readonly RegressionMapControl rasterizingSkpMap;
    private static readonly RegressionMapControl rasterizingTilingSkpMap;
    private static readonly MapRenderer mapRenderer;
    private static readonly MapRenderer mapRendererWithoutCache;
    private readonly SKCanvas skCanvas;
    private readonly SKImageInfo imageInfo;
    private readonly SKSurface surface;

    static RenderToCpuPerformance()
    {
        mapRenderer = new MapRenderer();
        mapRendererWithoutCache = new MapRenderer();
        mapRendererWithoutCache.RenderCache.VectorCache = new NonCachingVectorCache(mapRendererWithoutCache.RenderCache.SymbolCache);
        tilingSkpMap = CreateMapControl(RenderFormat.Skp);
        tilingPngMap = CreateMapControl(RenderFormat.Png);
        tilingWebpMap = CreateMapControl(RenderFormat.WebP);
        rasterizingPngMap = CreateMapControl(RenderFormat.Png, false, true);
        rasterizingSkpMap = CreateMapControl(RenderFormat.Skp, false, true);
        rasterizingTilingSkpMap = CreateMapControl(RenderFormat.Skp, true, true);
        map = CreateMapControl();
    }

    public RenderToCpuPerformance()
    {
        imageInfo = new SKImageInfo((int)Math.Round(800 * 1.0), (int)Math.Round(600 * 1.0),
            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

        surface = SKSurface.Create(imageInfo);
        skCanvas = surface.Canvas;
    }

    public static RegressionMapControl CreateMapControl(RenderFormat? renderFormat = null, bool tiling = true, bool rasterizing = false)
    {
        var mapControl = new RegressionMapControl();
        mapControl.SetSize(800, 600);

        mapControl.Map = CreateMap(renderFormat, tiling, rasterizing);

        // zoom to correct Zoom level
        var resolution = ZoomHelper.ZoomOut(mapControl.Map.Resolutions, mapControl.Viewport.Resolution);
        mapControl.Navigator?.ZoomTo(resolution);

        // fetch data first time
        var fetchInfo = new FetchInfo(mapControl.Viewport.Extent!, mapControl.Viewport.Resolution, mapControl.Map?.CRS);
        mapControl.Map?.RefreshData(fetchInfo);
        mapControl.Map?.Layers.WaitForLoadingAsync().Wait();

        return mapControl;
    }

    private static Map CreateMap(RenderFormat? renderFormat = null, bool tiling = true, bool rasterizing = false)
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
            if (tiling)
            {
                var sqliteCache = new SqlitePersistentCache("Performance" + renderFormat);
                sqliteCache.Clear();
                layer = new RasterizingTileLayer(layer, persistentCache: sqliteCache, renderFormat: renderFormat.Value);
            }

            if (rasterizing)
            {
                layer = new RasterizingLayer(layer, renderFormat: renderFormat.Value);
            }
        }

        map.Layers.Add(layer);

        var extent = map.Layers[0].Extent!;
        map.Home = n => n.NavigateTo(extent);

        return map;
    }

    private static string GetAppDir()
    {
        var path = Path.GetDirectoryName(typeof(RenderToCpuPerformance).Assembly.Location)!;
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
        return new GradientTheme("PopDens", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
    }

    [Benchmark]
    public async Task RenderDefaultWithoutCacheAsync()
    {
        await map.WaitForLoadingAsync();
        mapRendererWithoutCache.Render(skCanvas, map.Viewport, map.Map!.Layers, map.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderDefaultAsync()
    {
        await map.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, map.Viewport, map.Map!.Layers, map.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderRasterizingPngAsync()
    {
        await rasterizingPngMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, rasterizingPngMap.Viewport, rasterizingPngMap.Map!.Layers, rasterizingPngMap.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderRasterizingSkpAsync()
    {
        await rasterizingSkpMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, rasterizingSkpMap.Viewport, rasterizingSkpMap.Map!.Layers, rasterizingSkpMap.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderRasterizingTilingSkpAsync()
    {
        await rasterizingTilingSkpMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, rasterizingTilingSkpMap.Viewport, rasterizingTilingSkpMap.Map!.Layers, rasterizingTilingSkpMap.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderTilingPngAsync()
    {
        await tilingPngMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, tilingPngMap.Viewport, tilingPngMap.Map!.Layers, tilingPngMap.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderTilingWebPAsync()
    {
        await tilingWebpMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, tilingWebpMap.Viewport, tilingWebpMap.Map!.Layers, tilingWebpMap.Map!.Widgets, Color.White);
    }

    [Benchmark]
    public async Task RenderTilingSkpAsync()
    {
        await tilingSkpMap.WaitForLoadingAsync();
        mapRenderer.Render(skCanvas, tilingSkpMap.Viewport, tilingSkpMap.Map!.Layers, tilingSkpMap.Map!.Widgets, Color.White);
    }

    public void Dispose()
    {
        skCanvas.Dispose();
        surface.Dispose();
    }
}
