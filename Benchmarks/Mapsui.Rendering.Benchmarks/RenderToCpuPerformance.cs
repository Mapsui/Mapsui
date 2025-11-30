using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Mapsui.Extensions;
using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using SkiaSharp;

#pragma warning disable IDE0005
#pragma warning disable IDISP001
#pragma warning disable IDISP003
#pragma warning disable IDISP004

namespace Mapsui.Rendering.Benchmarks;

[SimpleJob(RunStrategy.Throughput, iterationCount: 1, warmupCount: 0, invocationCount: 333, launchCount: 1)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public sealed class RenderToCpuPerformance : IDisposable
{
    private static readonly RegressionMapControl _tilingSkpMap;
    private static readonly RegressionMapControl _tilingPngMap;
    private static readonly RegressionMapControl _tilingWebpMap;
    private static readonly RegressionMapControl _map;
    private static readonly RegressionMapControl _rasterizingPngMap;
    private static readonly RegressionMapControl _rasterizingSkpMap;
    private static readonly RegressionMapControl _rasterizingTilingSkpMap;
    private static readonly MapRenderer _mapRenderer;
    private readonly SKCanvas _skCanvas;
    private readonly SKImageInfo _imageInfo;
    private readonly SKSurface _surface;
    private static readonly RenderService _renderServiceWithoutCache = new();

    static RenderToCpuPerformance()
    {
        _mapRenderer = new MapRenderer();
        _tilingSkpMap = CreateMapControlAsync(RenderFormat.Skp).Result;
        _tilingPngMap = CreateMapControlAsync(RenderFormat.Png).Result;
        _tilingWebpMap = CreateMapControlAsync(RenderFormat.WebP).Result;
        _rasterizingPngMap = CreateMapControlAsync(RenderFormat.Png, false, true).Result;
        _rasterizingSkpMap = CreateMapControlAsync(RenderFormat.Skp, false, true).Result;
        _rasterizingTilingSkpMap = CreateMapControlAsync(RenderFormat.Skp, true, true).Result;
        _map = CreateMapControlAsync().Result;
        _map.WaitForLoadingAsync().Wait();
        _tilingSkpMap.WaitForLoadingAsync().Wait();
        _tilingPngMap.WaitForLoadingAsync().Wait();
        _tilingWebpMap.WaitForLoadingAsync().Wait();
        _rasterizingPngMap.WaitForLoadingAsync().Wait();
        _rasterizingSkpMap.WaitForLoadingAsync().Wait();
        _rasterizingTilingSkpMap.WaitForLoadingAsync().Wait();
        _renderServiceWithoutCache.VectorCache.Enabled = false;
    }

    public RenderToCpuPerformance()
    {
        _imageInfo = new SKImageInfo((int)Math.Round(800 * 1.0), (int)Math.Round(600 * 1.0),
            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

        _surface = SKSurface.Create(_imageInfo);
        _skCanvas = _surface.Canvas;
    }

    public static async Task<RegressionMapControl> CreateMapControlAsync(RenderFormat? renderFormat = null, bool tiling = true, bool rasterizing = false)
    {
        var mapControl = new RegressionMapControl(_mapRenderer);
        mapControl.SetSize(800, 600);

        mapControl.Map = CreateMap(renderFormat, tiling, rasterizing);

        // zoom to correct Zoom level
        mapControl.Map.Navigator.ZoomOut();

        // fetch data first time
        mapControl.Map.RefreshData();
        _ = await mapControl.Map.Layers.WaitForLoadingAsync();

        return mapControl;
    }

    private static Map CreateMap(RenderFormat? renderFormat = null, bool tiling = true, bool rasterizing = false)
    {
        var map = new Map();

        var countrySource = new ShapeFile(GetAppDir() + $"{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}countries.shp", true)
        {
            CRS = "EPSG:4326"
        };
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

        var extent = map.Layers.Get(0).Extent!;
        map.Navigator.ZoomToBox(extent);

        return map;
    }

    private static string GetAppDir()
    {
        var path = Path.GetDirectoryName(typeof(RenderToCpuPerformance).Assembly.Location)!;
        return path;
    }

    private static Layer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateCountryTheme()
        };
    }

    private static GradientTheme CreateCountryTheme()
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
    public void RenderDefaultWithoutCache()
    {
        _mapRenderer.Render(_skCanvas, _map.Map.Navigator.Viewport, _map.Map.Layers, _map.Map.Widgets, _renderServiceWithoutCache, Color.White);
    }

    [Benchmark]
    public void RenderDefault()
    {
        _mapRenderer.Render(_skCanvas, _map.Map.Navigator.Viewport, _map.Map.Layers, _map.Map.Widgets, _map.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderRasterizingPng()
    {
        _mapRenderer.Render(_skCanvas, _rasterizingPngMap.Map.Navigator.Viewport, _rasterizingPngMap.Map.Layers, _rasterizingPngMap.Map.Widgets, _rasterizingPngMap.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderRasterizingSkp()
    {
        _mapRenderer.Render(_skCanvas, _rasterizingSkpMap.Map.Navigator.Viewport, _rasterizingSkpMap.Map.Layers, _rasterizingSkpMap.Map.Widgets, _rasterizingSkpMap.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderRasterizingTilingSkp()
    {
        _mapRenderer.Render(_skCanvas, _rasterizingTilingSkpMap.Map.Navigator.Viewport, _rasterizingTilingSkpMap.Map.Layers, _rasterizingTilingSkpMap.Map.Widgets, _rasterizingTilingSkpMap.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderTilingPng()
    {
        _mapRenderer.Render(_skCanvas, _tilingPngMap.Map.Navigator.Viewport, _tilingPngMap.Map.Layers, _tilingPngMap.Map.Widgets, _tilingPngMap.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderTilingWebP()
    {
        _mapRenderer.Render(_skCanvas, _tilingWebpMap.Map.Navigator.Viewport, _tilingWebpMap.Map.Layers, _tilingWebpMap.Map.Widgets, _tilingWebpMap.Map.RenderService, Color.White);
    }

    [Benchmark]
    public void RenderTilingSkp()
    {
        _mapRenderer.Render(_skCanvas, _tilingSkpMap.Map.Navigator.Viewport, _tilingSkpMap.Map.Layers, _tilingSkpMap.Map.Widgets, _tilingSkpMap.Map.RenderService, Color.White);
    }

    public void Dispose()
    {
        _skCanvas.Dispose();
        _surface.Dispose();
    }
}
