using Mapsui.Manipulations;
using System;
using System.Threading.Tasks;

namespace Mapsui;

public class MapInfoEventArgs(Func<MapInfo> getMapInfo, Func<Task<MapInfo>> getMapInfoAsync, TapType tapType, bool handled) : EventArgs
{
    /// <summary>
    /// Number of times the user tapped the location
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;

    /// <summary>
    /// Function to get the map info
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; } = getMapInfo;
    public Func<Task<MapInfo>> GetMapInfoAsync { get; } = getMapInfoAsync;
}
