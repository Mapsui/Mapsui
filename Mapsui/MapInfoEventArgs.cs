using Mapsui.Manipulations;
using System;
using System.Threading.Tasks;

namespace Mapsui;

public class MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, Func<MapInfo> getMapInfo,
    Func<Task<MapInfo>> getRemoteMapInfoAsync, TapType tapType, bool handled)
        : BaseEventArgs(screenPosition, worldPosition, tapType, getMapInfo)
{
    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;

    /// <summary>
    /// Function to get the remote map info
    /// </summary>
    public Func<Task<MapInfo>> GetRemoteMapInfoAsync { get; } = getRemoteMapInfoAsync;
}
