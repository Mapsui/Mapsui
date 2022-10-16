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

#pragma warning disable IDISP001
#pragma warning disable IDISP003

namespace Mapsui.Rendering.Benchmarks
{
    [SimpleJob(RunStrategy.Throughput)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class RenderToBitmapPerformance
    {
        private static readonly RegressionMapControl skpMap;
        private static readonly RegressionMapControl pngMap;
        private static readonly RegressionMapControl webpMap;
        private static readonly RegressionMapControl map;
        private static readonly MapRenderer mapRenderer;

        static RenderToBitmapPerformance()
        {
            mapRenderer = new MapRenderer();
            skpMap = CreateMapControl(RenderFormat.Skp);            
            pngMap = CreateMapControl(RenderFormat.Png);
            webpMap = CreateMapControl(RenderFormat.WebP);
            map = CreateMapControl();
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
                layer = new RasterizingTileLayer(layer, persistentCache: new SqlitePersistentCache("Performance" + renderFormat), renderFormat: renderFormat.Value);
            }

            map.Layers.Add(layer);

            var extent = map.Layers[0].Extent!;
            map.Home = n => n.NavigateTo(extent);

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
            return new GradientTheme("PopDens", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
        }
        
        [Benchmark]
        public async Task RenderDefaultAsync()
        {
            await map.WaitForLoadingAsync();
            using var bitmap = mapRenderer.RenderToBitmapStream(map.Viewport, map.Map!.Layers, Color.White);
#if DEBUG
            File.WriteAllBytes(@$"{OutputFolder()}\Test.png", bitmap.ToArray());
#endif
        }

        [Benchmark]
        public async Task RenderRasterizingTilingPngAsync()
        { 
            await pngMap.WaitForLoadingAsync();
            using var bitmap = mapRenderer.RenderToBitmapStream(pngMap.Viewport, pngMap.Map!.Layers, Color.White);
#if DEBUG
            File.WriteAllBytes(@$"{OutputFolder()}\Testpng.png", bitmap.ToArray());
#endif
        }

        [Benchmark]
        public async Task RenderRasterizingTilingWebPAsync()
        {
            await webpMap.WaitForLoadingAsync();
            using var bitmap = mapRenderer.RenderToBitmapStream(webpMap.Viewport, webpMap.Map!.Layers, Color.White);
#if DEBUG
            File.WriteAllBytes(@$"{OutputFolder()}\Testwebp.png", bitmap.ToArray());
#endif
        }
        
        [Benchmark]
        public async Task RenderRasterizingTilingSkpAsync()
        {
            await skpMap.WaitForLoadingAsync();
            using var bitmap = mapRenderer.RenderToBitmapStream(skpMap.Viewport, skpMap.Map!.Layers, Color.White);
#if DEBUG
            File.WriteAllBytes(@$"{OutputFolder()}\Testskp.png", bitmap.ToArray());
#endif
        }              

#if DEBUG
        private string OutputFolder()
        {
            var path = Path.GetDirectoryName(typeof(RenderToBitmapPerformance).Assembly.Location);
            return path;
        }
#endif
    }
}
