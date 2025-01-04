using Mapsui.Manipulations;
using System;

namespace Mapsui;

public class BaseEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType, Func<MapInfo> getMapInfo) : EventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public ScreenPosition Position { get; } = screenPosition;

    /// <summary>
    /// World Position of touch in map coordinates
    /// </summary>
    public MPoint WorldPosition { get; } = worldPosition;

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// Function to get the MapInfo for the WidgetEventArgs.Position.
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; } = getMapInfo;
}
