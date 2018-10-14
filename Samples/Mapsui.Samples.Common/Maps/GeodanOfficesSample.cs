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
            var imageStream = EmbeddedResourceLoader.Load("Images.location.png", typeof(GeodanOfficesSample));

            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(new[] {geodanAmsterdam, geodanDenBosch}),
                Style = new SymbolStyle
                {
                    BitmapId = BitmapRegistry.Instance.Register(imageStream),
                    SymbolOffset = new Offset {Y = 64},
                    SymbolScale = 0.25
                },
                CRS = "EPSG:28992",
                Name = "Geodan Offices"
            };
            return layer;
        }
    }
}