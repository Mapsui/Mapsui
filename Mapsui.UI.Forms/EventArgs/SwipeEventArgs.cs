using System;

namespace Mapsui.UI.Forms
{
    public class SwipeEventArgs : EventArgs
    {
        public double VelocityX { get; } // Velocity in pixel/second
        public double  VelocityY { get; } // Velocity in pixel/second
        public bool Handled { get; set; }

        public SwipeEventArgs(double velocityX, double velocityY, bool handled)
        {
            VelocityX = velocityX;
            VelocityY = velocityY;
            Handled = Handled;
        }
    }
}
