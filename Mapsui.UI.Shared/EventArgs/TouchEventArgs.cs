using System;
using System.Collections.Generic;

namespace Mapsui.UI
{
    public class TouchEventArgs : EventArgs
    {
        public List<Geometries.Point> TouchPoints { get; }
        public bool Handled { get; set; }

        public TouchEventArgs(List<Geometries.Point> touchPoints, bool handled)
        {
            TouchPoints = touchPoints;
            Handled = Handled;
        }
    }
}
