using System;

namespace Mapsui.UI.Forms
{
    public sealed class MapClickedEventArgs : EventArgs
    {
        public Position Point { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; } = false;

        internal MapClickedEventArgs(Position point, int numOfTaps)
        {
            Point = point;
            NumOfTaps = numOfTaps;
        }
    }
}