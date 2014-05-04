using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointLayerWithWorldUnitsForSymbolsSample
    {
        public static IProvider Create()
        {
            var netherlands = new Feature { Geometry = new Point(710000, 6800000)};

            const string resource = "Mapsui.Samples.Common.Images.netherlands.jpg";
            netherlands.Styles.Add(new SymbolStyle
            {
                Symbol = new Bitmap { Data = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) },
                SymbolType = SymbolType.Rectangle,
                UnitType = UnitType.WorldUnit,
                SymbolRotation = 5f,
                SymbolScale = 1400
            });

            return new MemoryProvider(netherlands);
        }
    }
}
