using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class GeodanOfficesSample
    {
        public static MemoryLayer CreateLayer()
        {
            var geodanAmsterdam = new Geometries.Point(122698, 483922);
            var geodanDenBosch = new Geometries.Point(148949, 411446);
            var location = typeof(GeodanOfficesSample).LoadBitmapId("Images.location.png");

            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider<IFeature>(new[] { geodanAmsterdam, geodanDenBosch }.ToFeatures()),
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