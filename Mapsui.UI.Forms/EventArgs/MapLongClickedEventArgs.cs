using System;

namespace Mapsui.UI.Forms
{
    public sealed class MapLongClickedEventArgs : EventArgs
    {
        public Position Point { get; }
        public bool Handled { get; set; } = false;

        internal MapLongClickedEventArgs(Position point)
        {
            Point = point;
        }
    }
}