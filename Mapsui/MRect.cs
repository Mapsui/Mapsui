using System;
using System.Collections.Generic;

namespace Mapsui;

public class MRect
{
    public MRect(double minX, double minY, double maxX, double maxY)
    {
        Min = new MPoint(minX, minY);
        Max = new MPoint(maxX, maxY);
        EnforceMinMax();
    }

    public MRect(MRect rect) : this(rect.Min.X, rect.Min.Y, rect.Max.X, rect.Max.Y) { }

    public MRect(IEnumerable<MRect> rects)
    {
        foreach (var rect in rects)
        {
            if (Min is null) Min = rect.Min.Copy();
            if (Max is null) Max = rect.Max.Copy();

            Min.X = Math.Min(Min.X, rect.Min.X);
            Min.Y = Math.Min(Min.Y, rect.Min.Y);
            Max.X = Math.Max(Max.X, rect.Max.X);
            Max.Y = Math.Max(Max.Y, rect.Max.Y);
        }

        if (Min == null) throw new ArgumentException("Empty Collection", nameof(rects));
        if (Max == null) throw new ArgumentException("Empty Collection", nameof(rects));
    }

    public MPoint Max { get; }
    public MPoint Min { get; }

    public double MaxX => Max.X;
    public double MaxY => Max.Y;
    public double MinX => Min.X;
    public double MinY => Min.Y;

    public MPoint Centroid => new MPoint(Min.X + Width * 0.5, Min.Y + Height * 0.5);

    public double Width => Max.X - Min.X;
    public double Height => Max.Y - Min.Y;

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

    protected bool Equals(MRect? other)
    {
        if (other == null)
            return false;

        return Max.Equals(other.Max) && Min.Equals(other.Min);
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
        EnforceMinMax();
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
        var box = Copy();
        box.Min.X -= change;
        box.Min.Y -= change;
        box.Max.X += change;
        box.Max.Y += change;
        return box;
    }

    /// <summary>
    ///     Calculates a new quad by rotating this rect about its center by the
    ///     specified angle clockwise
    /// </summary>
    /// <param name="degrees">Angle about which to rotate (degrees)</param>
    /// <returns>Returns the calculated quad</returns>
    public MQuad Rotate(double degrees)
    {
        var bottomLeft = new MPoint(MinX, MinY);
        var topLeft = new MPoint(MinX, MaxY);
        var topRight = new MPoint(MaxX, MaxY);
        var bottomRight = new MPoint(MaxX, MinY);
        var quad = new MQuad(bottomLeft, topLeft, topRight, bottomRight);
        var center = Centroid;

        return quad.Rotate(degrees, center.X, center.Y);
    }

    private void EnforceMinMax()
    {
        if (Min.X > Max.X)
        {
            (Min.X, Max.X) = (Max.X, Min.X);
        }
        if (Min.Y > Max.Y)
        {
            (Min.Y, Max.Y) = (Max.Y, Min.Y);
        }
    }

    /// <summary>
    ///     Returns a string representation of the vertices from bottom-left and top-right
    /// </summary>
    /// <returns>Returns the string</returns>
    public override string ToString()
    {
        return $"BL: {BottomLeft}  TR: {TopRight}";
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((MRect)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Max.GetHashCode() * 397) ^ Min.GetHashCode();
        }
    }


}
