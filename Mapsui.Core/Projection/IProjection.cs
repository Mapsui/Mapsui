namespace Mapsui.Projection
{
    public interface IProjection
    {
        (double X, double Y) Project(string fromCRS, string toCRS, double x, double y);
        bool IsProjectionSupported(string fromCRS, string toCRS);
    }
}