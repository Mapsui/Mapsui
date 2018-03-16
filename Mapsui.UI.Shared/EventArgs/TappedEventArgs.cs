using System;

namespace Mapsui.UI
{
    public class TappedEventArgs : EventArgs
    {
        public Geometries.Point ScreenPosition { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; } = false;

        public TappedEventArgs(Geometries.Point screenPosition, int numOfTaps)
        {
            ScreenPosition = screenPosition;
            NumOfTaps = numOfTaps;
        }
    }
}
