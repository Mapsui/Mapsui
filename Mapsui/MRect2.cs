using System;
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
        if (point is null) return false;

        if (point.X < Min.X) return false;
        if (point.Y < Min.Y) return false;
        if (point.X > Max.X) return false;
        if (point.Y > Max.Y) return false;

        return true;
    }

    public bool Contains(MRect r)
    {
        return Min.X <= r.Min.X && Min.Y <= r.Min.Y && Max.X >= r.Max.X && Max.Y >= r.Max.Y;
    }

    public bool Equals(MRect? other)
    {
        if (other is null) return false;

        return Min.Equals(other.Min) && Max.Equals(other.Max);
    }

    public double GetArea()
    {
        return Width * Height;
    }

    public MRect Grow(double amount)
    {
        return Grow(amount, amount);
    }

    public MRect Grow(double amountInX, double amountInY)
    {
        var grownBox = new MRect(Min.X - amountInX, Min.Y - amountInY, Max.X + amountInX, MaxY + amountInY);
        // Todo: Check min max
        return grownBox;
    }


    public bool Intersects(MRect? rect)
    {
        if (rect is null) return false;

        if (rect.Max.X < Min.X) return false;
        if (rect.Max.Y < Min.Y) return false;
        if (rect.Min.X > Max.X) return false;
        if (rect.Min.Y > Max.Y) return false;

        return true;
    }

    public MRect Join(MRect? rect)
    {
        if (rect is null) return Copy();

        return new MRect(
            Math.Min(Min.X, rect.Min.X),
            Math.Min(Min.Y, rect.Min.Y),
            Math.Max(Max.X, rect.Max.X),
            Math.Max(Max.Y, rect.Max.Y));
    }

    /// <summary>
    /// Adjusts the size by increasing Width and Heigh with (Width * Height) / 2 * factor.
    /// </summary>
    /// <param name="factor"></param>
    /// <returns></returns>
    public MRect Multiply(double factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException($"{nameof(factor)} can not be smaller than zero");
        }

        var size = (Width + Height) * 0.5;
        var change = (size * 0.5 * factor) - (size * 0.5);
        var box = Clone();
        box.Min.X -= change;
        box.Min.Y -= change;
        box.Max.X += change;
        box.Max.Y += change;
        return box;
    }

    //MQuad Rotate(double degrees);
}
