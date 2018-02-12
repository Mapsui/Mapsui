using System;

namespace Mapsui.UI.Forms
{
    public sealed class MapClickedEventArgs : EventArgs
    {
        public Position Point { get; }
        public bool Handled { get; set; } = false;

        internal MapClickedEventArgs(Position point)
        {
            Point = point;
        }
    }
}