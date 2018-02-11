using System;

namespace Mapsui.UI.Forms
{
    public class TapEventArgs : EventArgs
    {
        public Geometries.Point Location { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; }

        public TapEventArgs(Geometries.Point location, int numOfTaps, bool handled)
        {
            Location = location;
            Handled = handled;
        }
    }
}
