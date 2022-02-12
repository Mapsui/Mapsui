using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Wpf.Editing.Extensions;

internal static class CoordinateExtensions
{
    public static void SetXY(this Coordinate? target, Coordinate? source)
    {
        if (target is null) return;
        if (source is null) return;

        target.X = source.X;
        target.Y = source.Y;
    }

    public static void SetXY(this Coordinate? target, MPoint? source)
    {
        if (target is null) return;
        if (source is null) return;

        target.X = source.X;
        target.Y = source.Y;
    }
}
