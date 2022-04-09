using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Samples.Common.Maps.Animations;

public class AnimatedPointsSamples : ISample
{
    public string Name => "Animated Points";

    public string Category => "Animations";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateAnimatedPointLayer());
        return map;
    }

    private static ILayer CreateAnimatedPointLayer()
    {
        return new AnimatedPointLayer(new AnimatedPointsSampleProvider())
        {
            Name = "Animated Points",
            Style = CreatePointStyle()
        };
    }

    private static IStyle CreatePointStyle()
    {
        return new ThemeStyle(f => {
            return CreateSvgArrowStyle("Images.arrow.svg", 0.5, f);
        });
    }

    private static IStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
    {
        var bitmapId = typeof(SvgSample).LoadSvgId(embeddedResourcePath);
        return new SymbolStyle
        {
            BitmapId = bitmapId,
            SymbolScale = scale,
            SymbolOffset = new Offset(0.0, 0.5, true),
            Opacity = 0.5f,
            Fill = null, // If the Fill.Color is set this will override the color of the svg itself
            SymbolRotation = (double)feature["rotation"]!
        };
    }
}
