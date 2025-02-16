using Mapsui.Manipulations;

namespace Mapsui;

public class MapEventArgs : BaseEventArgs
{
    public MapEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType, Viewport viewport,
        GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
        : base(screenPosition, worldPosition, gestureType, viewport, getMapInfo, getRemoteMapInfoAsync)
    {
    }
}
