namespace Mapsui;

public class MapBuilder
{
#pragma warning disable IDISP006
    readonly Map _map = new Map { CRS = "EPSG:3857" };
#pragma warning restore IDISP006

    /// <summary>
    /// Underlying map object
    /// </summary>
    public Map Map => _map;

    /// <summary>
    /// Build the Map object
    /// </summary>
    /// <returns></returns>
    public Map Build() { return _map; }

    /// <summary>
    /// Create a new MapBuilder
    /// </summary>
    /// <returns>New created MapBuilder</returns>
    public static MapBuilder Create() 
    { 
        return new MapBuilder(); 
    }
}
