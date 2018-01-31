using Mapsui.Geometries;
using System;

namespace Mapsui.Widgets
{
    public class WidgetTouchedEventArgs : EventArgs
    {
        public WidgetTouchedEventArgs(Point position)
        {
            Position = position;
        }

        public Point Position { get; }
    }
}
