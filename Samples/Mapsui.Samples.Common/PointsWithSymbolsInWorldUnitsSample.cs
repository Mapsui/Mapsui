using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointsWithSymbolsInWorldUnitsSample
    {
        public static ILayer CreateLayer()
        {
            return new Layer("PointLayer WorldUnits")
            {
                DataSource = CreateProvider()
            };
        }

        public static IProvider CreateProvider()
        {
            var netherlands = new Feature { Geometry = new Point(710000, 6800000)};

            const string resource = "Mapsui.Samples.Common.Images.netherlands.jpg";
            var assembly = typeof(PointsWithSymbolsInWorldUnitsSample).GetTypeInfo().Assembly;
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
    }
}
