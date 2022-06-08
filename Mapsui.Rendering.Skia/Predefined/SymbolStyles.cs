using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Styles;

public static class SymbolStyles
{
    public static SymbolStyle CreatePinStyle(Color? pinColor = null, double symbolScale = 1.0)
    {
        // This method is in Mapsui.Rendering.Skia because it has a dependency on Skia
        // because the resource is converted to an image using Skia. I think
        // It should be possible to create a style with just a reference to the platform
        // independent resource. The conversion to an image should happen in a render phase that
        // precedes a paint phase. https://github.com/Mapsui/Mapsui/issues/1448
        var pinId = typeof(Map).LoadSvgId("Resources.Images.Pin.svg");
        return new SymbolStyle
        {
            BitmapId = pinId,
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            SymbolScale = symbolScale,
            BlendModeColor = pinColor ?? Color.FromArgb(255, 57, 115, 199) // Determines color of the pin
        };
    }
}
