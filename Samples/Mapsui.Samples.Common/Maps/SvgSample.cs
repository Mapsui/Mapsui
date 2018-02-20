using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class SvgSample
    {
        private const string SvgLayerName = "Svg Layer";
        
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateSvgLayer(map.Envelope));
            map.HoverLayers.Add(map.Layers.First(l => l.Name == SvgLayerName));

            return map;
        }

        private static ILayer CreateSvgLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                Name = SvgLayerName,
                DataSource = CreateMemoryProviderWithDiverseSymbols(envelope, 2000),
                Style = null
            };
        }

        public static MemoryProvider CreateMemoryProviderWithDiverseSymbols(BoundingBox envelope, int count = 100)
        {
            return new MemoryProvider(CreateSvgFeatures(PointsSample.GenerateRandomPoints(envelope, count, 3)));
        }

        private static Features CreateSvgFeatures(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var counter = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point, ["Label"] = counter.ToString() };

                //feature.Styles.Add(CreateSvgStyle("Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg", 0.05));
                //feature.Styles.Add(CreateBitmapStyle("Mapsui.Samples.Common.Images.Ghostscript_Tiger.png", 0.05));

                feature.Styles.Add(CreateSvgStyle("Mapsui.Samples.Common.Images.Pin.svg", 0.5));
                //feature.Styles.Add(CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.5));

                features.Add(feature);
                counter++;
            }
            return features;
        }

        private static IStyle SmalleDot()
        {
            return new SymbolStyle { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };
        }

        private static IEnumerable<IStyle> CreateDiverseStyles()
        {
            const int diameter = 16;
            return new List<IStyle>
            {
                new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0,0), SymbolType = SymbolType.Rectangle},
                new SymbolStyle {SymbolScale = 0.6, SymbolOffset = new Offset(diameter,diameter), SymbolType = SymbolType.Rectangle, Fill = new Brush(Color.Red)},
                new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter,-diameter), SymbolType = SymbolType.Rectangle},
                new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(-diameter,-diameter), SymbolType = SymbolType.Rectangle},
                new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0,0)},
                new SymbolStyle {SymbolScale = 1.2, SymbolOffset = new Offset(diameter, 0)},
                new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(0, diameter)},
                new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter, diameter)},
                CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.7),
                CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.8),
                CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.9),
                CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 1.0),
                CreateSvgStyle("Mapsui.Samples.Common.Images.Pin.svg", 0.7),
                CreateSvgStyle("Mapsui.Samples.Common.Images.Pin.svg", 0.8),
                CreateSvgStyle("Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg", 0.05),
                CreateSvgStyle("Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg", 0.1),
            };
        }

        private static SymbolStyle CreateBitmapStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        }

        private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolType = SymbolType.Svg, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream(imagePath);
            var bitmapId = BitmapRegistry.Instance.Register(image);
            return bitmapId;
        }

        private static IFeature CreatePointWithStackedStyles()
        {
            var feature = new Feature { Geometry = new Point(5000000, -5000000) };

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 2.0f,
                Fill = null,
                Outline = new Pen { Color = Color.Yellow }
            });

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.8f,
                Fill = new Brush { Color = Color.Red }
            });

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.5f,
                Fill = new Brush { Color = Color.Black }
            });

            feature.Styles.Add(new LabelStyle
            {
                Text = "Stacked Styles",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
            });

            return feature;
        }


    }
}