using Mapsui.Manipulations;
using System;
using System.Threading.Tasks;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType, bool leftButton,
    bool shiftPressed, Func<MapInfo> getMapInfo, Func<Task<MapInfo>> getRemoteMapInfoAsync)
    : BaseEventArgs(screenPosition, worldPosition, tapType, getMapInfo, getRemoteMapInfoAsync)
{
    /// <summary>
    /// Left button used while touching
    /// </summary>
    public bool LeftButton { get; } = leftButton;

    /// <summary>
    /// Shift key pressed while touching
    /// </summary>
    public bool ShiftPressed { get; } = shiftPressed;
}
