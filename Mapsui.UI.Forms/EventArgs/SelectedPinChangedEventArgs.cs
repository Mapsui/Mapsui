using System;

namespace Mapsui.UI.Forms
{
    public sealed class SelectedPinChangedEventArgs : EventArgs
    {
        public Pin SelectedPin
        {
            get;
            private set;
        }

        internal SelectedPinChangedEventArgs(Pin selectedPin)
        {
            SelectedPin = selectedPin;
        }
    }
}
