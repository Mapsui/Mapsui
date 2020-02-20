
namespace Mapsui.Styles
{
    public enum UnitType
    {
        Pixel,
        WorldUnit
    }

    public class ImageStyle : VectorStyle
    {
        public ImageStyle()
        {
            BitmapId = -1;
            SymbolOffset = new Offset();
            SymbolScale = 1f;
        }

        public UnitType UnitType { get; set; }


        /// <summary>
        ///     Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public double SymbolRotation { get; set; }
        
        /// <summary>
        ///     This identifies bitmap in the BitmapRegistry.
        /// </summary>
        public int BitmapId { get; set; }

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
    }
}
