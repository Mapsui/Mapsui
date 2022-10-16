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

#pragma warning disable IDISP001
#pragma warning disable IDISP003

namespace Mapsui.Rendering.Benchmarks
{
    [SimpleJob(RunStrategy.Throughput)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class RenderToCpuPerformance
    {
        private static readonly RegressionMapControl skpMap;
        private static readonly RegressionMapControl pngMap;
        private static readonly RegressionMapControl webpMap;
        private static readonly RegressionMapControl map;
        private static readonly MapRenderer mapRenderer;
        private readonly SKCanvas skCanvas;
        private readonly SKImageInfo imageInfo;
        private readonly SKSurface surface;

        static RenderToCpuPerformance()
        {
            mapRenderer = new MapRenderer();
            skpMap = CreateMapControl(RenderFormat.Skp);            
            pngMap = CreateMapControl(RenderFormat.Png);
            webpMap = CreateMapControl(RenderFormat.WebP);
            map = CreateMapControl();
        }

        public RenderToCpuPerformance()
        {
            imageInfo = new SKImageInfo((int)Math.Round(800 * 1.0), (int)Math.Round(600 * 1.0),
                SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

            surface = SKSurface.Create(imageInfo);
            skCanvas = surface.Canvas;
        }
        
        public static RegressionMapControl CreateMapControl(RenderFormat? renderFormat = null)
        {
            var mapControl = new RegressionMapControl();
            mapControl.SetSize(800, 600);
            
            mapControl.Map = CreateMap(renderFormat);

            // fetch data first time
            var fetchInfo = new FetchInfo(mapControl.Viewport.Extent!, mapControl.Viewport.Resolution, mapControl.Map?.CRS);
            mapControl.Map?.RefreshData(fetchInfo);
            mapControl.Map?.Layers.WaitForLoadingAsync().Wait();
            
            return mapControl;
        }

        private static Map CreateMap(RenderFormat? renderFormat = null)
        {
            var map = new Map();

            var countrySource = new ShapeFile(GetAppDir() + $"{Path.PathSeparator}Data{Path.PathSeparator}countries.shp", true);
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
                layer = new RasterizingTileLayer(layer, persistentCache: new SqlitePersistentCache("Performance" + renderFormat), renderFormat: renderFormat.Value);
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
        public async Task RenderDefaultAsync()
        {
            await map.WaitForLoadingAsync();
            mapRenderer.Render(skCanvas, map.Viewport, map.Map!.Layers, map.Map!.Widgets, Color.White);
        }

        [Benchmark]
        public async Task RenderRasterizingTilingPngAsync()
        { 
            await pngMap.WaitForLoadingAsync();
            mapRenderer.Render(skCanvas, pngMap.Viewport, pngMap.Map!.Layers, pngMap.Map!.Widgets, Color.White);
        }

        [Benchmark]
        public async Task RenderRasterizingTilingWebPAsync()
        {
            await webpMap.WaitForLoadingAsync();
            mapRenderer.Render(skCanvas, webpMap.Viewport, webpMap.Map!.Layers, webpMap.Map!.Widgets, Color.White);
        }
        
        [Benchmark]
        public async Task RenderRasterizingTilingSkpAsync()
        {
            await skpMap.WaitForLoadingAsync();
            mapRenderer.Render(skCanvas, skpMap.Viewport, skpMap.Map!.Layers, skpMap.Map!.Widgets, Color.White);
        }
    }
}
