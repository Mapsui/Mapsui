using System;
using Mapsui.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
{
    public enum SymbolType
    {
        Ellipse,
        Rectangle,
        Triangle,
        Bitmap,
        Svg
    }

    public enum UnitType
    {
        Pixel,
        WorldUnit
    }

    public class SymbolStyle : VectorStyle
    {
        public static double DefaultWidth { get; set; } = 32;

        public static double DefaultHeight { get; set; } = 32;

        public SymbolStyle()
        {
            SymbolOffset = new Offset();
            SymbolScale = 1f;
            BitmapId = -1;
        }

        /// <summary>
        ///     This identifies bitmap in the BitmapRegistry.
        /// </summary>
        public int BitmapId { get; set; }

        /// <summary>
        ///     Scale of the symbol (defaults to 1)
        /// </summary>
        /// <remarks>
        ///     Setting the symbolscale to '2.0' doubles the size of the symbol, where a scale of 0.5 makes the scale half the size
        ///     of the original image
        /// </remarks>
        public double SymbolScale { get; set; }

        /// <summary>
        ///     Gets or sets the offset in pixels of the symbol.
        /// </summary>
        /// <remarks>
        ///     The symbol offset is scaled with the <see cref="SymbolScale" /> property and refers to the offset of
        ///     <see cref="SymbolScale" />=1.0.
        /// </remarks>
        public Offset SymbolOffset { get; set; }

        /// <summary>
        ///     Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public double SymbolRotation { get; set; }

        /// <summary>
        /// When true a symbol will rotate along with the rotation of the map.
        /// The is useful if you need to symbolize the direction in which a vehicle
        /// is moving. When the symbol is false it will retain it's position to the
        /// screen. This is useful for pins like symbols. The default is false.
        /// </summary>
        public bool RotateWithMap { get; set; }

        public UnitType UnitType { get; set; }

        public SymbolType SymbolType { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SymbolStyle))
                return false;
            return Equals((SymbolStyle)obj);
        }

        public bool Equals(SymbolStyle symbolStyle)
        {
            if (!base.Equals(symbolStyle))
                return false;

            if (BitmapId != symbolStyle.BitmapId)
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

            if (Math.Abs(Opacity - symbolStyle.Opacity) > Constants.Epsilon)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return
                BitmapId.GetHashCode() ^
                SymbolScale.GetHashCode() ^
                SymbolOffset.GetHashCode() ^
                SymbolRotation.GetHashCode() ^
                UnitType.GetHashCode() ^
                SymbolType.GetHashCode() ^
                base.GetHashCode();
        }

        public static bool operator ==(SymbolStyle symbolStyle1, SymbolStyle symbolStyle2)
        {
            return Equals(symbolStyle1, symbolStyle2);
        }

        public static bool operator !=(SymbolStyle symbolStyle1, SymbolStyle symbolStyle2)
        {
            return !Equals(symbolStyle1, symbolStyle2);
        }
    }
}