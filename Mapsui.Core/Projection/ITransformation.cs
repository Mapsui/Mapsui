namespace Mapsui.Projection
{
    public interface ITransformation
    {
        (double X, double Y) Transform(string fromCRS, string toCRS, double x, double y);
        bool IsProjectionSupported(string fromCRS, string toCRS);
    }
}