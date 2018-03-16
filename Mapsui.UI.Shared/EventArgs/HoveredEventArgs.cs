using System;

namespace Mapsui.UI
{
    public class HoveredEventArgs : EventArgs
    {
        public Geometries.Point ScreenPosition { get; }
        public bool Handled { get; set; } = false;

        public HoveredEventArgs(Geometries.Point screenPosition)
        {
            ScreenPosition = screenPosition;
        }
    }
}
