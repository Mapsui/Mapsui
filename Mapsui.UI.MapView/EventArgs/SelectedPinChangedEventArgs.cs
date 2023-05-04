using System;

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public sealed class SelectedPinChangedEventArgs : EventArgs
{
    /// <summary>
    /// Pin that was selected
    /// </summary>
    public Pin SelectedPin { get; }

    internal SelectedPinChangedEventArgs(Pin selectedPin)
    {
        SelectedPin = selectedPin;
    }
}
