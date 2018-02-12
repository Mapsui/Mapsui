using System;

namespace Mapsui.UI
{
    public class HoverEventArgs : EventArgs
    {
        public Geometries.Point ScreenPosition { get; }
        public bool Handled { get; set; } = false;

        public HoverEventArgs(Geometries.Point screenPosition)
        {
            ScreenPosition = screenPosition;
        }
    }
}
