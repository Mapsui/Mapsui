namespace Mapsui.Tiling.Extensions;

public static class MapExtensions
{
    /// <summary>
    /// Add a OSM layer to map
    /// </summary>
    /// <param name="map">Map to add layer</param>
    /// <param name="userAgent">UserAgent to use</param>
    /// <returns>Map</returns>
    public static Map AddOsmLayer(this Map map, string? userAgent = null)
    {
        map.Layers.Add(OpenStreetMap.CreateTileLayer(userAgent));

        return map;
    }
}
