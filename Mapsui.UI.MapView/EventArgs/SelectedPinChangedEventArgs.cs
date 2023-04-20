using System;

#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#else
namespace Mapsui.UI;
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
