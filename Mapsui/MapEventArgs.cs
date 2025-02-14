using Mapsui.Manipulations;

namespace Mapsui;

public class MapEventArgs : BaseEventArgs
{
    public MapEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType, Viewport viewport,
        GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
        : base(screenPosition, worldPosition, tapType, viewport, getMapInfo, getRemoteMapInfoAsync)
    {
    }
}
