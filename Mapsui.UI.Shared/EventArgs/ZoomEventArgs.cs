using System;

namespace Mapsui.UI
{
    public class ZoomEventArgs : EventArgs
    {
        public Geometries.Point ScreenPosition { get; }
        public ZoomDirection Direction { get; }
        public bool Handled { get; set; }

        public ZoomEventArgs(Geometries.Point screenPosition, ZoomDirection direction, bool handled)
        {
            ScreenPosition = screenPosition;
            Direction = direction;
            Handled = Handled;
        }
    }
}
