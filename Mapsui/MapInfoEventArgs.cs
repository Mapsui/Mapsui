using Mapsui.Manipulations;

namespace Mapsui;

public class MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType,
    Map map, GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
        : BaseEventArgs(screenPosition, worldPosition, gestureType, map, getMapInfo, getRemoteMapInfoAsync)
{ }
