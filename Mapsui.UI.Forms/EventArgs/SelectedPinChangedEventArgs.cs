using System;

namespace Mapsui.UI.Forms
{
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
}