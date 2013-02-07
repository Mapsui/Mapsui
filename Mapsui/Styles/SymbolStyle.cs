
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
        public SymbolStyle()
        {
            SymbolOffset = new Offset();
            SymbolScale = 1f;
            Opacity = 1f;
            Width = 32;
            Height = 32;
        }

        /// <summary>
        /// Symbol used for rendering points
        /// </summary>
        public Bitmap Symbol { get; set; }

        /// <summary>
        /// Scale of the symbol (defaults to 1)
        /// </summary>
        /// <remarks>
        /// Setting the symbolscale to '2.0' doubles the size of the symbol, where a scale of 0.5 makes the scale half the size of the original image
        /// </remarks>
        public double SymbolScale { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels of the symbol.
        /// </summary>
        /// <remarks>
        /// The symbol offset is scaled with the <see cref="SymbolScale"/> property and refers to the offset af <see cref="SymbolScale"/>=1.0.
        /// </remarks>
        public Offset SymbolOffset { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public double SymbolRotation { get; set; }

        public UnitType UnitType { get; set; }

        public SymbolType SymbolType { get; set; }

        public double Opacity { get; set; }
        
        public double Width { get; set; }

        public double Height { get; set; }

        public const double DefaultWidth = 48;

        public const double DefaultHeight = 48;

        #region Equals operator

        public override bool Equals(object obj)
        {
            if (!(obj is SymbolStyle))
            {
                return false;
            }
            return Equals((SymbolStyle)obj);
        }

        public bool Equals(SymbolStyle symbolStyle)
        {
            if (!base.Equals(symbolStyle))
            {
                return false;
            }

            if ((Symbol == null) ^ (symbolStyle.Symbol == null))
            {
                return false;
            }

            if (Symbol != null && !Symbol.Equals(symbolStyle.Symbol))
            {
                return false;
            }

            if (!SymbolScale.Equals(SymbolScale))
            {
                return false;
            }

            if ((SymbolOffset == null) ^ (symbolStyle.SymbolOffset == null))
            {
                return false;
            }

            if ((SymbolOffset != null) && (!SymbolOffset.Equals(symbolStyle.SymbolOffset)))
            {
                return false;
            }

            if (SymbolRotation != symbolStyle.SymbolRotation)
            {
                return false;
            }

            if (UnitType != symbolStyle.UnitType)
            {
                return false;
            }

            if (SymbolType != symbolStyle.SymbolType)
            {
                return false;
            }

            if (Opacity != symbolStyle.Opacity)
            {
                return false;
            }

            if (Width != symbolStyle.Width)
            {
                return false;
            }

            if (Height != symbolStyle.Height)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (Symbol == null ? 0 : Symbol.GetHashCode()) ^ 
                SymbolScale.GetHashCode() ^ SymbolOffset.GetHashCode() ^
                SymbolRotation.GetHashCode() ^ UnitType.GetHashCode() ^ SymbolType.GetHashCode() ^
                Opacity.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode() ^ base.GetHashCode();
        }

        public static bool operator ==(SymbolStyle symbolStyle1, SymbolStyle symbolStyle2)
        {
            return Equals(symbolStyle1, symbolStyle2);
        }

        public static bool operator !=(SymbolStyle symbolStyle1, SymbolStyle symbolStyle2)
        {
            return !Equals(symbolStyle1, symbolStyle2);
        }

        #endregion
    }
}
