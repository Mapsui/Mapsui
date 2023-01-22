using System;

namespace Mapsui.UI;

public class ZoomedEventArgs : EventArgs
{
    public MPoint ScreenPosition { get; }
    public ZoomDirection Direction { get; }
    public bool Handled { get; set; } = false;

    public ZoomedEventArgs(MPoint screenPosition, ZoomDirection direction)
    {
        ScreenPosition = screenPosition;
        Direction = direction;
    }
}
