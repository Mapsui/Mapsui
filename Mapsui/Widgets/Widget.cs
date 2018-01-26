using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    public abstract class Widget : IWidget
    {
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;
        public float PositionX { get; set; } = 0;
        public float PositionY { get; set; } = 0;
        public float MarginX { get; set; } = 2;
        public float MarginY { get; set; } = 2;
        public BoundingBox Envelope { get; set; }
    }
}