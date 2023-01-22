// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles;

public class VectorStyle : Style
{
    public VectorStyle()
    {
        Outline = new Pen { Color = Color.Gray, Width = 1 };
        Line = new Pen { Color = Color.Black, Width = 1 };
        Fill = new Brush { Color = Color.White };
    }
    /// <summary>
    /// Line style for line geometries
    /// </summary>
    public Pen? Line { get; set; }

    /// <summary>
    /// Outline style for line and polygon geometries
    /// </summary>
    public Pen? Outline { get; set; }

    /// <summary>
    /// Fill style for Polygon geometries
    /// </summary>
    public Brush? Fill { get; set; }

    public override bool Equals(object? obj)
    {
        if (!(obj is VectorStyle style))
        {
            return false;
        }
        return Equals(style);
    }

    public bool Equals(VectorStyle? vectorStyle)
    {
        if (vectorStyle == null)
            return false;

        if (!base.Equals(vectorStyle))
            return false;

        if (!Line?.Equals(vectorStyle.Line) ?? false)
            return false;

        if (!Outline?.Equals(vectorStyle.Outline) ?? false)
            return false;

        if (!Fill?.Equals(vectorStyle.Fill) ?? false)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return Line?.GetHashCode() ?? 0
            ^ Outline?.GetHashCode() ?? 0
            ^ Fill?.GetHashCode() ?? 0
            ^ base.GetHashCode();
    }

    public static bool operator ==(VectorStyle? vectorStyle1, VectorStyle? vectorStyle2)
    {
        return Equals(vectorStyle1, vectorStyle2);
    }

    public static bool operator !=(VectorStyle? vectorStyle1, VectorStyle? vectorStyle2)
    {
        return !Equals(vectorStyle1, vectorStyle2);
    }
}
