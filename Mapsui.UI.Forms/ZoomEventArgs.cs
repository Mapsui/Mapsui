using System;

namespace Mapsui.UI.Forms
{
    public class ZoomEventArgs : EventArgs
    {
        public Geometries.Point Location { get; }
        public ZoomDirection Direction { get; }
        public bool Handled { get; set; }

        public ZoomEventArgs(Geometries.Point location, ZoomDirection direction, bool handled)
        {
            Location = location;
            Direction = direction;
            Handled = Handled;
        }
    }
}
