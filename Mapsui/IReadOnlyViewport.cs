using Mapsui.Geometries;

namespace Mapsui
{
    public interface IReadOnlyViewport 
    {
        Point Center { get; }
        double Resolution { get; set; }
        BoundingBox Extent { get; }
        double Width { get; }
        double Height { get; }
        double Rotation { get; }
        Point ScreenToWorld(Point screenPosition);
        Point ScreenToWorld(double x, double y);
        void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY,
            double deltaScale = 1, double deltaRotation = 0);
    }
}
