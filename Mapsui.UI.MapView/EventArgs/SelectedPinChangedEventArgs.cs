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
    public IPin SelectedPin { get; }

    internal SelectedPinChangedEventArgs(IPin selectedPin)
    {
        SelectedPin = selectedPin;
    }
}
