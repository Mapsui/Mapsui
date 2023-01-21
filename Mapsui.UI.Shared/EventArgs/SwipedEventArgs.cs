using System;

namespace Mapsui.UI;

public class SwipedEventArgs : EventArgs
{
    public double VelocityX { get; } // Velocity in pixel/second
    public double VelocityY { get; } // Velocity in pixel/second
    public bool Handled { get; set; } = false;

    public SwipedEventArgs(double velocityX, double velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }
}
