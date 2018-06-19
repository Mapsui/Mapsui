using System;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class DrawableClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Point of click in EPSG:4326 coordinates
        /// </summary>
        /// <value>The point.</value>
        public Position Point { get; }

        /// <summary>
        /// Point of click in screen coordinates
        /// </summary>
        /// <value>The screen point.</value>
        public Point ScreenPoint { get; }

        /// <summary>
        /// Number of taps
        /// </summary>
        /// <value>The number of taps.</value>
        public int NumOfTaps { get; }

        /// <summary>
        /// Flag, if this event was handled
        /// </summary>
        /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
        public bool Handled { get; set; } = false;

        internal DrawableClickedEventArgs(Position point, Point screenPoint, int numOfTaps)
        {
            Point = point;
            ScreenPoint = screenPoint;
            NumOfTaps = numOfTaps;
        }
    }
}