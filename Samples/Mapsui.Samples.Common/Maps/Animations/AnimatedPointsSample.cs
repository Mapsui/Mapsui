using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Animations;

public class AnimatedPointsSample : ISample
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

    private static ThemeStyle CreatePointStyle() => new(f =>
    {
        return CreateSvgArrowStyle("embeddedresource://Mapsui.Samples.Common.Images.arrow.svg", 0.5, f);
    });

    private static SymbolStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
    {
        return new SymbolStyle
        {
            BitmapPath = new Uri(embeddedResourcePath),
            SymbolScale = scale,
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            Opacity = 0.5f,
            SymbolRotation = (double)feature["rotation"]!
        };
    }
}
