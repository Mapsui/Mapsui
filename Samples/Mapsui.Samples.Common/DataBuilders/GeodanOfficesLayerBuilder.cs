using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.DataBuilders;

public class GeodanOfficesLayerBuilder
{
    public static MemoryLayer Create()
    {
        var geodanAmsterdam = new MPoint(122698, 483922);
        var geodanDenBosch = new MPoint(148949, 411446);
        var imageSource = "embedded://Mapsui.Samples.Common.Images.location.png";

        var layer = new MemoryLayer
        {
            Features = new[] { geodanAmsterdam, geodanDenBosch }.ToFeatures(),
            Style = new SymbolStyle
            {
                ImageSource = imageSource,
                SymbolOffset = new Offset { Y = 64 },
                SymbolScale = 0.25
            },
            Name = "Geodan Offices"
        };
        return layer;
    }
}
