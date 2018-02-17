using System;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class DrawableClickedEventArgs : EventArgs
    {
        public Position Point { get; }
        public Point ScreenPoint { get; }
        public int NumOfTaps { get; }
        public bool Handled { get; set; } = false;

        internal DrawableClickedEventArgs(Position point, Point screenPoint, int numOfTaps)
        {
            Point = point;
            ScreenPoint = screenPoint;
            NumOfTaps = numOfTaps;
        }
    }
}