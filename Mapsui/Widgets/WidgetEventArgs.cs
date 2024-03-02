using System;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs(MPoint position, int tapCount = 1, bool leftButton = true, bool shift = false) : EventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public MPoint Position { get; } = position;

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public int TapCount { get; } = tapCount;

    /// <summary>
    /// Left button used while touching
    /// </summary>
    public bool LeftButton { get; } = leftButton;

    /// <summary>
    /// Shift key pressed while touching
    /// </summary>
    public bool Shift { get; } = shift;
}
