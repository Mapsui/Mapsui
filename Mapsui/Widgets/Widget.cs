using System;

namespace Mapsui.Widgets;

public abstract class Widget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;
    public double MarginX { get; set; } = 2;
    public double MarginY { get; set; } = 2;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public virtual double Width { get; set; }
    public virtual double Height { get; set; }
    public MRect? Envelope { get; set; }
    public bool Enabled { get; set; } = true;

    public double CalculatePositionX(double left, double right, double width)
    {
        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Left:
                return MarginX;

            case HorizontalAlignment.Center:
                return (right - left - width) / 2;

            case HorizontalAlignment.Right:
                return right - left - width - MarginX;
        }

        throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment);
    }

    public double CalculatePositionY(double top, double bottom, double height)
    {
        switch (VerticalAlignment)
        {
            case VerticalAlignment.Top:
                return MarginY;

            case VerticalAlignment.Bottom:
                return bottom - top - height - MarginY;

            case VerticalAlignment.Center:
                return (bottom - top - height) / 2;
        }

        throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment);
    }
}
