using System;
using Mapsui.Utilities;

namespace Mapsui.Styles;

public enum SymbolType
{
    Ellipse,
    Rectangle,
    Triangle,
}

public enum UnitType
{
    Pixel,
    WorldUnit
}

public class SymbolStyle : VectorStyle, IPointStyle
{
    public static double DefaultWidth { get; set; } = 32;
    public static double DefaultHeight { get; set; } = 32;

    public SymbolType SymbolType { get; set; }

    public UnitType UnitType { get; set; }

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

    [Obsolete("Use Offset or RelativeOffset instead", true)]
    public Offset SymbolOffset { get; set; } = new Offset();

    /// <summary>
    ///     Gets or sets the offset in pixels of the symbol.
    /// </summary>
    /// <remarks>
    ///     The symbol offset is scaled with the <see cref="SymbolScale" /> property and refers to the offset of
    ///     <see cref="SymbolScale" />=1.0.
    /// </remarks>
    public Offset Offset { get; set; } = new Offset();

    /// <summary>
    /// Offset of the symbol in units relative to the size of the symbol. When X = 0 and Y = 0 it will be centered.
    /// </summary>
    public RelativeOffset RelativeOffset { get; set; } = new RelativeOffset();

    /// <summary>
    /// Should SymbolOffset position rotate with map
    /// </summary>
    public bool SymbolOffsetRotatesWithMap { get; set; }

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

        if ((Offset == null) ^ (symbolStyle.Offset == null))
            return false;

        if ((Offset != null) && !Offset.Equals(symbolStyle.Offset))
            return false;

        if (Math.Abs(SymbolRotation - symbolStyle.SymbolRotation) > Constants.Epsilon)
            return false;

        if (UnitType != symbolStyle.UnitType)
            return false;

        if (SymbolType != symbolStyle.SymbolType)
            return false;

        if (Math.Abs(Opacity - symbolStyle.Opacity) > Constants.Epsilon)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return
            SymbolScale.GetHashCode() ^
            Offset.GetHashCode() ^
            SymbolRotation.GetHashCode() ^
            UnitType.GetHashCode() ^
            SymbolType.GetHashCode() ^
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
}
