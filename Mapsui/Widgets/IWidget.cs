using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    public interface IWidget
    {
        HorizontalAlignment HorizontalAlignment { get; set; }
        VerticalAlignment VerticalAlignment { get; set; }
        float MarginX { get; set; }
        float MarginY { get; set; }
        /// <summary>
        /// The hit box of the widget. This needs to be updated from the widget renderer.
        /// </summary>
        BoundingBox Envelope { get; set; }

        void HandleWidgetTouched(INavigator navigator, Point position);
    }
}
