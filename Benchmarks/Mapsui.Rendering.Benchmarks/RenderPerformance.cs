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
using Mapsui.Rendering.Skia.Tests;
using Mapsui.Styles.Thematics;

#pragma warning disable IDISP001
#pragma warning disable IDISP003

namespace Mapsui.Rendering.Benchmarks
{
    [SimpleJob(RunStrategy.Throughput, targetCount: 1)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class RenderPerformance
    {
        private readonly RegressionMapControl skpMap;
        private readonly RegressionMapControl pngMap;
        private readonly RegressionMapControl webpMap;
        private readonly RegressionMapControl map;

        public RenderPerformance()
        {
            skpMap = CreateMapControl(ERenderFormat.Skp);            
            pngMap = CreateMapControl(ERenderFormat.Png);
            webpMap = CreateMapControl(ERenderFormat.WebP);
            map = CreateMapControl();
        }
        
        public static RegressionMapControl CreateMapControl(ERenderFormat? renderFormat = null)
        {
            var mapControl = new RegressionMapControl();
            mapControl.SetSize(800, 600);
            
            mapControl.Map = CreateMap(renderFormat);

            // fetch data first time
            var fetchInfo = new FetchInfo(mapControl.Viewport.Extent!, mapControl.Viewport.Resolution, mapControl.Map?.CRS);
            mapControl.Map?.RefreshData(fetchInfo);
            
            return mapControl;
        }

        private static Map CreateMap(ERenderFormat? renderFormat = null)
        {
            var map = new Map();

            var countrySource = new ShapeFile(GetAppDir() + "\\Data\\countries.shp", true);
            countrySource.CRS = "EPSG:4326";
            var projectedCountrySource = new ProjectingProvider(countrySource)
            {
                CRS = "EPSG:3857",
            };

            ILayer layer = CreateCountryLayer(projectedCountrySource);
            if (renderFormat != null)
            {
                layer = new RasterizingTileLayer(layer, persistentCache: new MemoryPersistentCache(), renderFormat: renderFormat.Value);
            }

            map.Layers.Add(layer);

            var extent = map.Layers[0].Extent!;
            map.Home = n => n.NavigateTo(extent);

            return map;
        }

        private static string GetAppDir()
        {
            var path = Path.GetDirectoryName(typeof(RenderPerformance).Assembly.Location)!;
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
        public void RenderDefault()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Map!.Layers, Color.White);
        }

        [Benchmark]
        public void RenderRasterizingTilingPng()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(pngMap.Viewport, pngMap.Map!.Layers, Color.White);
        }

        [Benchmark]
        public void RenderRasterizingTilingWebP()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(webpMap.Viewport, webpMap.Map!.Layers, Color.White);
        }
        
        [Benchmark]
        public void RenderRasterizingTilingSkp()
        {
            using var bitmap = new MapRenderer().RenderToBitmapStream(skpMap.Viewport, skpMap.Map!.Layers, Color.White);
        }              
    }
}
