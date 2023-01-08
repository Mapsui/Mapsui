using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Samples.Common.Maps.Styles;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Utilities;
using System.Threading.Tasks;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Samples.Common.Maps.Animations;

public class AnimatedPointsSamples : ISample
{
    public string Name => "Animated Points";

    public string Category => "Animations";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateAnimatedPointLayer());
        return Task.FromResult(map);
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
        return new ThemeStyle(f =>
        {
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
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            Opacity = 0.5f,
            SymbolRotation = (double)feature["rotation"]!
        };
    }
}
