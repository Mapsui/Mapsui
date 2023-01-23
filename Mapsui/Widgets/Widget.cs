using System;

namespace Mapsui.Widgets;

public abstract class Widget : IWidget
{
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;
    public float MarginX { get; set; } = 2;
    public float MarginY { get; set; } = 2;
    public MRect? Envelope { get; set; }
    public bool Enabled { get; set; } = true;

    public float CalculatePositionX(float left, float right, float width)
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

    public float CalculatePositionY(float top, float bottom, float height)
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

    public abstract bool HandleWidgetTouched(INavigator navigator, MPoint position);
}
