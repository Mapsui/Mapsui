using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public class GeodanOfficesSample
    {
        public static MemoryLayer CreateLayer()
        {
            var geodanAmsterdam = new MPoint(122698, 483922);
            var geodanDenBosch = new MPoint(148949, 411446);
            var location = typeof(GeodanOfficesSample).LoadBitmapId("Images.location.png");

            var layer = new MemoryLayer
            {
                Features = new[] { geodanAmsterdam, geodanDenBosch }.ToFeatures(),
                Style = new SymbolStyle
                {
                    BitmapId = location,
                    SymbolOffset = new Offset { Y = 64 },
                    SymbolScale = 0.25
                },
                Name = "Geodan Offices"
            };
            return layer;
        }
    }
}
