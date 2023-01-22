using System;

namespace Mapsui.Widgets;

public class WidgetTouchedEventArgs : EventArgs
{
    public WidgetTouchedEventArgs(MPoint position)
    {
        Position = position;
    }

    public MPoint Position { get; }

    /// <summary>
    /// True, if this Widget had handled this event
    /// </summary>
    public bool Handled { get; set; }
}
