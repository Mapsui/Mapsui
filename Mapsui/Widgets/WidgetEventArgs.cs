using Mapsui.Manipulations;
using System;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs(ScreenPosition position, MPoint worldPosition, TapType tapType, bool leftButton, bool shiftPressed, Func<MapInfo> getMapInfo) : BaseEventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public ScreenPosition Position { get; } = position;

    /// <summary>
    /// World Position of touch in map coordinates
    /// </summary>
    public MPoint WorldPosition { get; } = worldPosition;

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// Left button used while touching
    /// </summary>
    public bool LeftButton { get; } = leftButton;

    /// <summary>
    /// Shift key pressed while touching
    /// </summary>
    public bool ShiftPressed { get; } = shiftPressed;

    /// <summary>
    /// Function to get the MapInfo for the WidgetEventArgs.Position.
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; } = getMapInfo;
}
