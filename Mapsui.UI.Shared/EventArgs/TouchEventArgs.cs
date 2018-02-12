using System;
using System.Collections.Generic;

namespace Mapsui.UI
{
    public class TouchEventArgs : EventArgs
    {
        public List<Geometries.Point> ScreenPoints { get; }
        public bool Handled { get; set; } = false;

        public TouchEventArgs(List<Geometries.Point> screenPoints)
        {
            ScreenPoints = screenPoints;
        }
    }
}
