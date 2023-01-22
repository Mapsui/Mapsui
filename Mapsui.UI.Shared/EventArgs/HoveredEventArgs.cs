using System;

namespace Mapsui.UI;

public class HoveredEventArgs : EventArgs
{
    public MPoint ScreenPosition { get; }
    public bool Handled { get; set; } = false;

    public HoveredEventArgs(MPoint screenPosition)
    {
        ScreenPosition = screenPosition;
    }
}
