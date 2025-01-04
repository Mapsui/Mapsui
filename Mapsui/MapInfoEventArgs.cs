using Mapsui.Manipulations;
using System;

namespace Mapsui;

public class MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, Func<MapInfo> getMapInfo,
    GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync, TapType tapType, bool handled)
        : BaseEventArgs(screenPosition, worldPosition, tapType, getMapInfo, getRemoteMapInfoAsync)
{
    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;
}
