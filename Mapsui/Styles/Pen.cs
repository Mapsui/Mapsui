// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles;

public class Pen
{
    public Pen() : this(Color.Transparent) { }

    public Pen(Color color, double width = 1)
    {
        Color = color;
        Width = width;
    }

    /// <summary>
    /// Width of line
    /// </summary>
    public double Width { get; set; } = 1;

    /// <summary>
    /// Color of line
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Style of the line (solid/dashed), which is drawn
    /// </summary>
    public PenStyle PenStyle { get; set; } = PenStyle.Solid;

    /// <summary>
    /// Array for drawing user defined dashes. Should be even and values are 
    /// multiplied by line width before drawing.
    /// </summary>
    public float[]? DashArray { get; set; } = null;

    /// <summary>
    /// Offset for drawing user defined dashes
    /// </summary>
    public float DashOffset { get; set; } = 0;

    /// <summary>
    /// Defines the end of a line
    /// </summary>
    public PenStrokeCap PenStrokeCap { get; set; } = PenStrokeCap.Butt;

    /// <summary>
    /// Defines how line parts are join together
    /// </summary>
    public StrokeJoin StrokeJoin { get; set; } = StrokeJoin.Round;

    /// <summary>
    /// Defines up to which width of line StrokeJoin is used
    /// </summary>
    public float StrokeMiterLimit { get; set; } = 10f; // Default on Wpf, on Skia, it is 4f

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

        return Equals((Pen)obj);
    }

    protected bool Equals(Pen? pen)
    {
        if (pen == null)
            return false;

        return Width.Equals(pen.Width) && Color.Equals(pen.Color) && PenStyle == pen.PenStyle && Equals(DashArray, pen.DashArray) && DashOffset.Equals(pen.DashOffset) && PenStrokeCap == pen.PenStrokeCap && StrokeJoin == pen.StrokeJoin && StrokeMiterLimit.Equals(pen.StrokeMiterLimit);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Width.GetHashCode();
            hashCode = (hashCode * 397) ^ Color.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)PenStyle;
            hashCode = (hashCode * 397) ^ (DashArray != null ? DashArray.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ DashOffset.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)PenStrokeCap;
            hashCode = (hashCode * 397) ^ (int)StrokeJoin;
            hashCode = (hashCode * 397) ^ StrokeMiterLimit.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Pen? pen1, Pen? pen2)
    {
        return Equals(pen1, pen2);
    }

    public static bool operator !=(Pen? pen1, Pen? pen2)
    {
        return !Equals(pen1, pen2);
    }


}
