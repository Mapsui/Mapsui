using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointLayerWithWorldUnitsForSymbolsSample
    {
        public static ILayer Create()
        {
            var layer = new Layer("PointLayer WorldUnits");
            var netherlands = new Feature { Geometry = new Point(710000, 6800000)};
            
            const string resource = "DemoConfig.Images.netherlands.jpg";
            netherlands.Styles.Add(new SymbolStyle
            {
                Symbol = new Bitmap { Data = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) },
                SymbolType = SymbolType.Rectangle,
                UnitType = UnitType.WorldUnit,
                SymbolRotation = 5f,
                SymbolScale = 1400,
                Width = 365,
                Height = 380
            });
            
            layer.DataSource = new MemoryProvider(new[] { netherlands });
            return layer;
        }
    }
}
