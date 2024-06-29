using Mapsui.Widgets.ScaleBar;
using static Mapsui.MapBuilder;

namespace Mapsui.Tiling.Extensions;

public static class MapBuilderExtensions
{
    public static MapBuilder WithOpenStreetMapLayer(this MapBuilder mapBuilder)
    {
        return mapBuilder.WithLayer(() => OpenStreetMap.CreateTileLayer());
    }

    /// <summary>
    /// This method is here to test extended methods for widgets. It should be in 
    /// the MapBuilder instead (because it can be, unlike those for the TileLayer)
    /// </summary>
    /// <param name="mapBuilder"></param>
    /// <returns></returns>
    public static MapBuilder WithScaleBarWidget(this MapBuilder mapBuilder, ConfigureWidget configureWidget)
    {
        return mapBuilder.WithWidget((m) => new ScaleBarWidget(m) { Margin = new MRect(16) }, configureWidget);
    }

    /// <summary>
    /// This method is here to test extended methods for widgets. It should be in 
    /// the MapBuilder instead (because it can be, unlike those for the TileLayer)
    /// </summary>
    /// <param name="mapBuilder"></param>
    /// <returns></returns>
    public static MapBuilder WithScaleBarWidget(this MapBuilder mapBuilder)
    {
        return mapBuilder.WithWidget((m) => new ScaleBarWidget(m) { Margin = new MRect(16) }, (w) => { });
    }
}
