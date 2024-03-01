using System;

namespace Mapsui.Widgets;

/// <summary>
/// Arguments for a touched event of a widget
/// </summary>
public class WidgetEventArgs : EventArgs
{
    public WidgetEventArgs(MPoint position, int clickCount = 1, bool leftButton = true, bool shift = false)
    {
        Position = position;
        ClickCount = clickCount;
        LeftButton = leftButton;
        Shift = shift;
    }

    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public MPoint Position { get; }

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public int ClickCount { get; }

    /// <summary>
    /// Left button used while touching
    /// </summary>
    public bool LeftButton { get; }

    /// <summary>
    /// Shift key pressed while touching
    /// </summary>
    public bool Shift { get; }
}
