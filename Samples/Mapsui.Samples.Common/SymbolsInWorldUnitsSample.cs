using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class SymbolsInWorldUnitsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(Create());
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new Layer("PointLayer WorldUnits") {DataSource = CreateProvider()};
        }

        public static IProvider CreateProvider()
        {
            var netherlands = new Feature {Geometry = new Point(710000, 6800000)};

            const string resource = "Mapsui.Samples.Common.Images.netherlands.jpg";
            var assembly = typeof(SymbolsInWorldUnitsSample).GetTypeInfo().Assembly;
            var bitmapDataStream = assembly.GetManifestResourceStream(resource);
            netherlands.Styles.Add(new SymbolStyle
            {
                BitmapId = BitmapRegistry.Instance.Register(bitmapDataStream),
                SymbolType = SymbolType.Rectangle,
                UnitType = UnitType.WorldUnit,
                SymbolRotation = 5f,
                SymbolScale = 1400
            });

            return new MemoryProvider(netherlands);
        }

        public static ILayer Create()
        {
            return new Layer("PointLayer")
            {
                DataSource = new MemoryProvider(new[]
                {
                    CreatePointWithLabel(),
                    CreatePointWithDefaultStyle(),
                    CreatePointWithSmallBlackDot()
                }),
                Style = null
            };
        }

        private static Feature CreatePointWithLabel()
        {
            var feature = new Feature { Geometry = new Point(0, 1000000) };
            feature.Styles.Add(new LabelStyle { Text = "Label" });
            return feature;
        }

        private static Feature CreatePointWithDefaultStyle()
        {
            var feature = new Feature { Geometry = new Point(1000000, 1000000) };
            feature.Styles.Add(new SymbolStyle());
            return feature;
        }

        private static IFeature CreatePointWithSmallBlackDot()
        {
            var feature = new Feature { Geometry = new Point(1000000, 0) };

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 2.0f,
                Fill = new Brush { Color = null },
                Outline = new Pen { Color = Color.Green }
            });

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.5f,
                Fill = new Brush { Color = Color.Black }
            });

            return feature;
        }
    }
}