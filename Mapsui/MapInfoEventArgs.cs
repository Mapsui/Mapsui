﻿using Mapsui.Manipulations;

namespace Mapsui;

public class MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType,
    Viewport viewport, bool handled, GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
        : BaseEventArgs(screenPosition, worldPosition, gestureType, viewport, getMapInfo, getRemoteMapInfoAsync)
{
    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;
}
