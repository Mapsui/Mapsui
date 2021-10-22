namespace Mapsui.Projection
{
    public interface IMapsuiTransformation : ITransformation
    {
        void Transform(string fromCRS, string toCRS, MPoint point);
        void Transform(string fromCRS, string toCRS, MRect rect);
    }
}