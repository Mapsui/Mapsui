namespace Mapsui.Extensions;

public static class TupleExtensions
{
    public static MPoint ToMPoint(this (double x, double y) coordinate)
    {
        return new MPoint(coordinate.x, coordinate.y);
    }
}
