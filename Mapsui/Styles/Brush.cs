// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue

using System;

namespace Mapsui.Styles;

public class Brush : IHasImage
{
    private Image? _image;

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
        FillStyle = brush.FillStyle;
    }

    public Color? Color { get; set; }

    // todo: 
    // Perhaps rename to something like SecondaryColor. The 'Color' 
    // field is itself a background in many cases. This is confusing
    public Color? Background { get; set; }

    /// <summary>
    /// If a bitmap is used as <see cref="Image"/> the <see cref="BitmapRegion"/> can be used to specific a 
    /// subregion that will be used as image symbol. This way the  <see cref="Image"/> can be used as an 'atlas'
    /// for 'sprites', which is a common mechanism in 2D gaming engines This will not affect SVGs.
    /// </summary>
    public BitmapRegion? BitmapRegion { get; set; }

    public Image? Image
    {
        get => _image;
        set
        {
            _image = value;

            if (_image != null)
            {
                if (!(FillStyle is FillStyle.Bitmap or FillStyle.BitmapRotated))
                {
                    FillStyle = FillStyle.Bitmap;
                }
            }
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

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Brush)obj);
    }

    protected bool Equals(Brush? brush)
    {
        if (brush == null)
            return false;

        return _image == brush._image && Equals(Color, brush.Color) && Equals(Background, brush.Background) && FillStyle == brush.FillStyle;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_image, Color, Background, FillStyle);
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
