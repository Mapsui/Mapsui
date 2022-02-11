using System.Collections.Generic;

namespace Mapsui;

public class MRect2
{
    public MPoint Max { get; }
    public MPoint Min { get; }

    public double MaxX => Max.X;
    public double MaxY => Max.Y;
    public double MinX => Min.X;
    public double MinY => Min.Y;

    public MPoint Centroid => new MPoint(Max.X - Min.X, Max.Y - Min.Y);

    public double Width => Max.X - MinX;
    public double Height => Max.Y - MinY;

    public double Bottom => Min.Y;
    public double Left => Min.X;
    public double Top => Max.Y;
    public double Right => Max.X;

    public MPoint TopLeft => new MPoint(Left, Top);
    public MPoint TopRight => new MPoint(Right, Top);
    public MPoint BottomLeft => new MPoint(Left, Bottom);
    public MPoint BottomRight => new MPoint(Right, Bottom);

    /// <summary>
    ///     Returns the vertices in clockwise order from bottom left around to bottom right
    /// </summary>
    public IEnumerable<MPoint> Vertices
    {
        get
        {
            yield return BottomLeft;
            yield return TopLeft;
            yield return TopRight;
            yield return BottomRight;
        }
    }

    public MRect Copy()
    {
        return new MRect(Min.X, Min.Y, Max.X, Max.Y);
    }

    public bool Contains(MPoint? point)
    {
        if (point == null) return false;

        if (point.X < Min.X) return false;
        if (point.Y < Min.Y) return false;
        if (point.X > Max.X) return false;
        if (point.Y > Max.Y) return false;

        return true;
    }

    //bool Contains(MRect r);
    //bool Equals(MRect? other);
    //double GetArea();
    //MRect Grow(double amount);
    //MRect Grow(double amountInX, double amountInY);
    //bool Intersects(MRect? box);
    //MRect Join(MRect? box);
    //MRect Multiply(double factor);
    //MQuad Rotate(double degrees);
}
