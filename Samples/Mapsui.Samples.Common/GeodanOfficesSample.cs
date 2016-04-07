using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public class GeodanOfficesSample
    {
        public static MemoryLayer CreateLayer()
        {
            var geodanAmsterdam = new Geometries.Point(122698, 483922);
            var geodanDenBosch = new Geometries.Point(148949, 411446);
            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(new [] { geodanAmsterdam, geodanDenBosch }),
                Style = new SymbolStyle { Fill = new Brush(Color.Red), SymbolScale = 1 },
                CRS = "EPSG:28992"
            };
            return layer;
        }
    }
}
