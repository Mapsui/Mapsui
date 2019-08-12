using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.UI
{
    public class DraggedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        public Geometries.Point PreviousPosition { get; }
        public Geometries.Point NewPosition { get; set; }

        public DraggedEventArgs()
        {
        }

        public DraggedEventArgs(Geometries.Point previousPosition, Geometries.Point newPosition)
        {
            PreviousPosition = previousPosition;
            NewPosition = newPosition;
        }
    }
}
