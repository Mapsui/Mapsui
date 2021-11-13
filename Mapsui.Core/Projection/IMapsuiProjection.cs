namespace Mapsui.Projection
{
    public interface IMapsuiProjection : IProjection
    {
        void Project(string fromCRS, string toCRS, MPoint point);
        void Project(string fromCRS, string toCRS, MRect rect);
    }
}