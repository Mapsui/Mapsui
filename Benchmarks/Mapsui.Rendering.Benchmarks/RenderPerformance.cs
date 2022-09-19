using BenchmarkDotNet.Disassemblers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using System.Reflection;
using BenchmarkDotNet.Engines;
using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Styles.Thematics;

namespace Mapsui.Rendering.Benchmarks
{
    [SimpleJob(RunStrategy.Throughput, targetCount: 1)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class RenderPerformance
    {
        private readonly LimitedViewport limitedViewport;

        private List<ILayer> skpLayers = new();
        private List<ILayer> pngLayers = new();
        private List<ILayer> webpLayers = new();
        private List<ILayer> layers = new();

        public RenderPerformance()
        {
            limitedViewport = new LimitedViewport();
            limitedViewport.Map = new Map();
            limitedViewport.SetSize(800, 800);
            limitedViewport.SetResolution(52545.912084609292);

            var countrySource = new ShapeFile(GetAppDir() + "\\Data\\countries.shp", true);
            countrySource.CRS = "EPSG:4326";
            var projectedCountrySource = new ProjectingProvider(countrySource)
            {
                CRS = "EPSG:3857",
            };

            skpLayers.Add(new RasterizingTileLayer(CreateCountryLayer(projectedCountrySource), persistentCache: new SqlitePersistentCache("countriesSkp"), renderFormat: ERenderFormat.Skp));
            pngLayers.Add(new RasterizingTileLayer(CreateCountryLayer(projectedCountrySource), persistentCache: new SqlitePersistentCache("countriesPng"), renderFormat: ERenderFormat.Png));
            webpLayers.Add(new RasterizingTileLayer(CreateCountryLayer(projectedCountrySource), persistentCache: new SqlitePersistentCache("countriesWebP"), renderFormat: ERenderFormat.WebP));
            layers.Add(CreateCountryLayer(projectedCountrySource));
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()!.GetModules()[0].FullyQualifiedName)!;
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
        public void RenderRasterizingTilingSkp()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(limitedViewport, skpLayers , Color.White, 2);
        }

        [Benchmark]
        public void RenderRasterizingTilingPng()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(limitedViewport, pngLayers , Color.White, 2);
        }

        [Benchmark]
        public void RenderRasterizingTilingWebP()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(limitedViewport, webpLayers , Color.White, 2);
        }

        [Benchmark]
        public void RenderRasterizing()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(limitedViewport, layers , Color.White, 2);
        }
    }
}
