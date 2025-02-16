using Mapsui.Manipulations;
using System;

namespace Mapsui.UI;

public class TappedEventArgs : EventArgs
{
    public ScreenPosition ScreenPosition { get; }
    public GestureType GestureType { get; }
    public bool Handled { get; set; } = false;

    public TappedEventArgs(ScreenPosition screenPosition, GestureType gestureType)
    {
        ScreenPosition = screenPosition;
        GestureType = gestureType;
    }
}
