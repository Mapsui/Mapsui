using System;
using Mapsui.UI.Objects;

namespace Mapsui.UI;

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
