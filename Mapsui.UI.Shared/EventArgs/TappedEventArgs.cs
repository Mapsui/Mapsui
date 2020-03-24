using System;

namespace Mapsui.UI
{
    public class TappedEventArgs : EventArgs
    {
        public Geometries.Point ScreenPosition { get; }
        public Geometries.Point ScreenPositionInPixels { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; } = false;

        public TappedEventArgs(Geometries.Point screenPosition, int numOfTaps) : this(screenPosition, null, numOfTaps)
        {
        }

        public TappedEventArgs(Geometries.Point screenPosition, Geometries.Point screenPositionInPixels, int numOfTaps)
        {
            ScreenPosition = screenPosition;
            ScreenPositionInPixels = screenPositionInPixels;
            NumOfTaps = numOfTaps;
        }
    }
}
