using System;
using System.IO;
using Mapsui.Utilities;

namespace Mapsui.Styles
{
    public enum SymbolType
    {
        Ellipse,
        Rectangle
    }

    public enum UnitType
    {
        Pixel,
        WorldUnit
    }

    public class SymbolStyle : VectorStyle
    {
        public const double DefaultWidth = 32;

        public const double DefaultHeight = 32;

        public SymbolStyle()
        {
            SymbolOffset = new Offset();
            SymbolScale = 1f;
            Opacity = 1f;
            BitmapId = -1;
        }

        /// <summary>
        ///     Symbol used for rendering points
        /// </summary>
        [Obsolete("Use BitmapID and BitmapRegistry instead", true)]
        public Bitmap Symbol
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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
        ///     The symbol offset is scaled with the <see cref="SymbolScale" /> property and refers to the offset af
        ///     <see cref="SymbolScale" />=1.0.
        /// </remarks>
        public Offset SymbolOffset { get; set; }

        /// <summary>
        ///     Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public double SymbolRotation { get; set; }

        public UnitType UnitType { get; set; }

        public SymbolType SymbolType { get; set; }

        public double Opacity { get; set; }

        private void Register(Stream data)
        {
            if (data != null) BitmapId = BitmapRegistry.Instance.Register(data);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SymbolStyle))
                return false;
            return Equals((SymbolStyle) obj);
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
                Opacity.GetHashCode() ^
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