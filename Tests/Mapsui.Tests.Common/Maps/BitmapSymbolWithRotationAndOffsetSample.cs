using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class BitmapSymbolWithRotationAndOffsetSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo(new Point(80, 80), 1)
            };
            var layer = new MemoryLayer
            {
                DataSource = CreateProviderWithRotatedBitmapSymbols(),
                Name = "Points with rotated bitmaps",
                Style = null
            };
            map.Layers.Add(layer);
            return map;
        }

        private static IProvider CreateProviderWithRotatedBitmapSymbols()
        {
            var features = new Features
            {
                new Feature
                {
                    Geometry = new Point(75, 75),
                    Styles = new[] {new SymbolStyle {Fill = new Brush(Color.Red)}}
                }, // for reference
                CreateFeatureWithRotatedBitmapSymbol(75, 125, 90),
                CreateFeatureWithRotatedBitmapSymbol(125, 125, 180),
                CreateFeatureWithRotatedBitmapSymbol(125, 75, 270)
            };
            return new MemoryProvider(features);
        }

        private static Feature CreateFeatureWithRotatedBitmapSymbol(double x, double y, double rotation)
        {
            const string bitmapPath = @"Mapsui.Tests.Common.Resources.Images.iconthatneedsoffset.png";
            var bitmapStream = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(bitmapPath);
            var bitmapId = BitmapRegistry.Instance.Register(bitmapStream);

            var feature = new Feature {Geometry = new Point(x, y)};

            feature.Styles.Add(new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolOffset = new Offset {Y = -24},
                SymbolRotation = rotation
            });
            return feature;
        }
    }
}