// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles;

public class Brush
{
    private int _bitmapId = -1;

    public Brush()
    {
    }

    public Brush(Color color)
    {
        Color = color;
    }

    public Brush(Brush brush)
    {
        Color = brush.Color;
        Background = brush.Background;
        BitmapId = brush.BitmapId;
        FillStyle = brush.FillStyle;
    }

    public Color? Color { get; set; }

    // todo: 
    // Perhaps rename to something like SecondaryColor. The 'Color' 
    // field is itself a background in many cases. This is confusing
    public Color? Background { get; set; }

    /// <summary>
    /// This identifies bitmap in the BitmapRegistry
    /// </summary>
    public int BitmapId
    {
        get => _bitmapId;
        set
        {
            _bitmapId = value;
            if (_bitmapId != -1 && !(FillStyle == FillStyle.Bitmap || FillStyle == FillStyle.BitmapRotated))
                FillStyle = FillStyle.Bitmap;
        }
    }

    /// <summary>
    /// This identifies how the brush is applied, works for Color not for bitmaps
    /// </summary>
    public FillStyle FillStyle { get; set; } = FillStyle.Solid;

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

        return Equals((Brush)obj);
    }

    protected bool Equals(Brush? brush)
    {
        if (brush == null)
            return false;

        return _bitmapId == brush._bitmapId && Equals(Color, brush.Color) && Equals(Background, brush.Background) && FillStyle == brush.FillStyle;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _bitmapId;
            hashCode = (hashCode * 397) ^ (Color != null ? Color.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Background != null ? Background.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)FillStyle;
            return hashCode;
        }
    }

    public static bool operator ==(Brush? brush1, Brush? brush2)
    {
        return Equals(brush1, brush2);
    }

    public static bool operator !=(Brush? brush1, Brush? brush2)
    {
        return !Equals(brush1, brush2);
    }


}
