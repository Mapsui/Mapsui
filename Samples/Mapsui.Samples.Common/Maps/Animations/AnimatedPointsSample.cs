using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
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

    private static AnimatedPointLayer CreateAnimatedPointLayer() => new(new AnimatedPointsSampleProvider())
    {
        Name = "Animated Points",
        Style = CreatePointStyle()
    };

    private static ThemeStyle CreatePointStyle() => new(CreateSvgArrowStyle);

    private static SymbolStyle CreateSvgArrowStyle(IFeature feature) => new()
    {
        ImageSource = "embedded://Mapsui.Samples.Common.Images.arrow.svg",
        SymbolScale = 0.5,
        SymbolOffset = new RelativeOffset(0.0, 0.5),
        Opacity = 0.5f,
        SymbolRotation = (double)feature["rotation"]!
    };
}
