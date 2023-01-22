using System;
using System.Collections.Generic;

namespace Mapsui.UI;

public class TouchedEventArgs : EventArgs
{
    public List<MPoint> ScreenPoints { get; }
    public bool Handled { get; set; } = false;

    public TouchedEventArgs(List<MPoint> screenPoints)
    {
        ScreenPoints = screenPoints;
    }
}
