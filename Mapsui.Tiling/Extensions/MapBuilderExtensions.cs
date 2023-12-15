namespace Mapsui.Tiling.Extensions;

public static class MapBuilderExtension
{
    /// <summary>
    /// Add an OSM layer to a MapBuilder
    /// </summary>
    /// <param name="mapBuilder">MapBuilder to use</param>
    /// <returns>MapBuilder</returns>
    public static MapBuilder AddOsmMap(this MapBuilder mapBuilder, string? userAgent = null)
    {
        var osmLayer = OpenStreetMap.CreateTileLayer(userAgent);

        mapBuilder.Map.Layers.Add(osmLayer);

        return mapBuilder;
    }
}
