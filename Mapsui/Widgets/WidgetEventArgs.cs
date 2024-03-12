using Mapsui.Manipulations;
using System;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs(ScreenPosition position, TapType tapType = TapType.Single, bool leftButton = true, bool shiftPressed = false) : EventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public ScreenPosition Position { get; } = position;

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
}
