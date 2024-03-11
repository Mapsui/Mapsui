using Mapsui.Manipulations;
using System;

namespace Mapsui.UI;

public class TappedEventArgs : EventArgs
{
    public ScreenPosition ScreenPosition { get; }
    public TapType TapType { get; }
    public bool Handled { get; set; } = false;

    public TappedEventArgs(ScreenPosition screenPosition, TapType tapType)
    {
        ScreenPosition = screenPosition;
        TapType = tapType;
    }
}
