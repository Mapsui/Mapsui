using System;

namespace Mapsui.UI;

public class TappedEventArgs : EventArgs
{
    public MPoint ScreenPosition { get; }
    public int NumOfTaps { get; }
    public bool Handled { get; set; } = false;

    public TappedEventArgs(MPoint screenPosition, int numOfTaps)
    {
        ScreenPosition = screenPosition;
        NumOfTaps = numOfTaps;
    }
}
