using Mapsui.Layers;

namespace Mapsui.Projection
{
    public interface IProjection
    {
        (double X, double Y) Project(string fromCRS, string toCRS, double x, double y);
        void Project(string fromCRS, string toCRS, MPoint point);
        void Project(string fromCRS, string toCRS, MRect rect);
        bool IsProjectionSupported(string fromCRS, string toCRS);
        void Project(string fromCRS, string toCRS, IFeature feature);
    }
}