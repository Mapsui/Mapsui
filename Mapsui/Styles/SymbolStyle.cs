using System;
using Mapsui.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles;

public enum SymbolType
{
    Ellipse,
    Rectangle,
    Triangle,
    Image
}

public enum UnitType
{
    Pixel,
    WorldUnit
}

public class SymbolStyle : VectorStyle, IHasImageSource
{
    public static double DefaultWidth { get; set; } = 32;
    public static double DefaultHeight { get; set; } = 32;

    public SymbolType SymbolType { get; set; }

    public UnitType UnitType { get; set; }

    private string? _imageSource;

    /// <summary>
    /// Path to the the image to display during rendering. This can be url, file path or embedded resource.
    /// </summary>
    public string? ImageSource
    {
        get => _imageSource;
        set
        {
            if (value != null)
                ValidateImageSource(value);
            _imageSource = value;
            if (value != null)
            {
                SymbolType = SymbolType.Image;
            }
        }
    }

    /// <summary>
    /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
    /// </summary>
    public double SymbolRotation { get; set; }

    /// <summary>
    /// When true a symbol will rotate along with the rotation of the map.
    /// This is useful if you need to symbolize the direction in which a vehicle
    /// is moving. When the symbol is false it will retain it's position to the
    /// screen. This is useful for pins like symbols. The default is false.
    /// This mode is not supported in the WPF renderer.
    /// </summary>
    public bool RotateWithMap { get; set; }

    /// <summary>
    ///     Scale of the symbol (defaults to 1)
    /// </summary>
    /// <remarks>
    ///     Setting the SymbolScale to '2.0' doubles the size of the symbol. A SymbolScale of 0.5 makes the scale half the size.
    ///     of the original image
    /// </remarks>
    public double SymbolScale { get; set; } = 1.0;

    /// <summary>
    ///     Gets or sets the offset in pixels of the symbol.
    /// </summary>
    /// <remarks>
    ///     The symbol offset is scaled with the <see cref="SymbolScale" /> property and refers to the offset of
    ///     <see cref="SymbolScale" />=1.0.
    /// </remarks>
    public Offset SymbolOffset { get; set; } = new Offset(0, 0);

    /// <summary>
    /// Should SymbolOffset position rotate with map
    /// </summary>
    public bool SymbolOffsetRotatesWithMap { get; set; }

    /// <summary>
    /// When BlendModeColor is set a BitmapType.Picture (e.g. used for SVG) will be 
    /// drawn in the BlendModeColor ignoring the colors of the Picture itself.
    /// </summary>
    public Color? BlendModeColor { get; set; }

    /// <summary>
    /// Option to override the fill color of the SVG image. This is useful if you want to change the color of the SVG 
    /// source image. Note that each different color used will add an new object to the image cache.
    /// </summary>
    public Color? SvgFillColor { get; set; }

    /// <summary>
    /// Option to override the stroke color of the SVG image. This is useful if you want to change the color of the SVG 
    /// source image. Note that each different color used will add an new object to the image cache.
    /// </summary>
    public Color? SvgStrokeColor { get; set; }

    /// <summary>
    /// Sets the sprite parameters used to specify which part of the image
    /// symbol should be used. This only applies if a ImageSource is set.
    /// </summary>
    public BitmapRegion? BitmapRegion { get; set; }

    public override bool Equals(object? obj)
    {
        if (!(obj is SymbolStyle style))
            return false;
        return Equals(style);
    }

    public bool Equals(SymbolStyle? symbolStyle)
    {
        if (symbolStyle == null)
            return false;

        if (!base.Equals(symbolStyle))
            return false;

        if (!SymbolScale.Equals(SymbolScale))
            return false;

        if ((SymbolOffset == null) ^ (symbolStyle.SymbolOffset == null))
            return false;

        if ((SymbolOffset != null) && !SymbolOffset.Equals(symbolStyle.SymbolOffset))
            return false;

        if (Math.Abs(SymbolRotation - symbolStyle.SymbolRotation) > Constants.Epsilon)
            return false;

        if (UnitType != symbolStyle.UnitType)
            return false;

        if (SymbolType != symbolStyle.SymbolType)
            return false;

        if (ImageSource != symbolStyle.ImageSource)
            return false;

        if (Math.Abs(Opacity - symbolStyle.Opacity) > Constants.Epsilon)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return
            ImageSource?.GetHashCode() ^
            SymbolScale.GetHashCode() ^
            SymbolOffset.GetHashCode() ^
            SymbolRotation.GetHashCode() ^
            UnitType.GetHashCode() ^
            SymbolType.GetHashCode() ^
            BlendModeColor?.GetHashCode() ?? 0 ^
            base.GetHashCode();
    }

    public static bool operator ==(SymbolStyle? symbolStyle1, SymbolStyle? symbolStyle2)
    {
        return Equals(symbolStyle1, symbolStyle2);
    }

    public static bool operator !=(SymbolStyle? symbolStyle1, SymbolStyle? symbolStyle2)
    {
        return !Equals(symbolStyle1, symbolStyle2);
    }

    private static void ValidateImageSource(string imageSource)
    {
        // Will throw a UriFormatException exception if the imageSource is not a valid Uri
        _ = new Uri(imageSource);
    }
}
