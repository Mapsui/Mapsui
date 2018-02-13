using Mapsui.Providers;
using System;

namespace Mapsui.UI.Forms
{
    public sealed class PinClickedEventArgs : EventArgs
    {
        public Pin Pin { get; }
        public Position Point { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; } = false;

        internal PinClickedEventArgs(Pin pin, Position point, int numOfTaps)
        {
            Pin = pin;
            Point = point;
            NumOfTaps = numOfTaps;
        }
    }
}