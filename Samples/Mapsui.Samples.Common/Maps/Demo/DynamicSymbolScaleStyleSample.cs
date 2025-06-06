using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class DynamicSymbolScaleStyleSample : ISample
{
    private const double _resolutionFlippingPoint = 300.0;
    private const string _imageSource = "embedded://Mapsui.Samples.Common.Images.arrow.svg";

    public string Name => "Dynamic Symbol Scale Style";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayerWithDynamicScaleStyle(map));
        return Task.FromResult(map);
    }

    private static MemoryLayer CreateLayerWithDynamicScaleStyle(Map map) => new()
    {
        Name = "Dynamic Symbol Scale",
        Features = RandomPointsBuilder.CreateRandomFeatures(map.Extent, 1000),
        Style = CreateDynamicSymbolScaleStyle()
    };

    private static ThemeStyle CreateDynamicSymbolScaleStyle()
    {
        return new ThemeStyle((f, v) => CreateImageStyle(v));
    }

    private static ImageStyle CreateImageStyle(Viewport v) => new()
    {
        Image = new Image { Source = _imageSource },
        SymbolScale = v.Resolution > _resolutionFlippingPoint ? 0.5 : 2.0,
    };
}
