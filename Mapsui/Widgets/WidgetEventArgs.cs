using Mapsui.Manipulations;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType, Viewport viewport,
    bool shiftPressed, GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync)
    : BaseEventArgs(screenPosition, worldPosition, tapType, viewport, getMapInfo, getRemoteMapInfoAsync)
{
    /// <summary>
    /// Shift key pressed while touching
    /// </summary>
    public bool ShiftPressed { get; } = shiftPressed;
}
