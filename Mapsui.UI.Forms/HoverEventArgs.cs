using System;

namespace Mapsui.UI.Forms
{
    public class HoverEventArgs : EventArgs
    {
        public Geometries.Point Location { get; }
        public bool Handled { get; set; }

        public HoverEventArgs(Geometries.Point location, bool handled)
        {
            Location = location;
            Handled = handled;
        }
    }
}
