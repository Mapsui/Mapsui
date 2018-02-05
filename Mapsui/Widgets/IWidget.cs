using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    public interface IWidget
    {
        HorizontalAlignment HorizontalAlignment { get; set; }
        VerticalAlignment VerticalAlignment { get; set; }
        float MarginX { get; set; }
        float MarginY { get; set; }
        BoundingBox Envelope { get; set; }

        void HandleWidgetTouched(Point position);
    }
}
