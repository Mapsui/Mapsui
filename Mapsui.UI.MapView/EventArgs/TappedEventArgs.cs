using Mapsui.Manipulations;
using System;

namespace Mapsui.UI;

public class TappedEventArgs : EventArgs
{
    public ScreenPosition ScreenPosition { get; }
    public int NumOfTaps { get; }
    public bool Handled { get; set; } = false;

    public TappedEventArgs(ScreenPosition screenPosition, int numOfTaps)
    {
        ScreenPosition = screenPosition;
        NumOfTaps = numOfTaps;
    }
}
