using Mapsui.Manipulations;

namespace Mapsui;

public class MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GetMapInfoDelegate getMapInfo,
    GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync, TapType tapType, Viewport viewport, bool handled)
        : BaseEventArgs(screenPosition, worldPosition, tapType, viewport, getMapInfo, getRemoteMapInfoAsync)
{
    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;
}
