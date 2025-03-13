using Mapsui.Manipulations;

namespace Mapsui;

public class MapEventArgs : BaseEventArgs
{
    public MapEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType, Map map,
        GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
        : base(screenPosition, worldPosition, gestureType, map, getMapInfo, getRemoteMapInfoAsync)
    {
    }
}
