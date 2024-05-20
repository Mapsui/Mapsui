using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Animations;

public sealed class AnimatedPointsSample : ISample, IDisposable
{
    private bool _disposed;
    readonly AnimatedPointsSampleProvider _animatedPointsSampleProvider = new();

    public string Name => "Animated Points";
    public string Category => "Animations";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateAnimatedPointLayer(_animatedPointsSampleProvider));
        return Task.FromResult(map);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _animatedPointsSampleProvider.Dispose();

        _disposed = true;
    }

    private static AnimatedPointLayer CreateAnimatedPointLayer(AnimatedPointsSampleProvider animatedPointsSampleProvider) => new(animatedPointsSampleProvider)
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
