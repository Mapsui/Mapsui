namespace Mapsui.Styles
{
    class BitmapStyle : SymbolStyle
    {
        /// <summary>
        ///     This identifies bitmap in the BitmapRegistry.
        /// </summary>
        public int BitmapId { get; set; } = -1;

        public override bool Equals(object obj)
        {
            if (!(obj is BitmapStyle))
                return false;
            return Equals((BitmapStyle)obj);
        }

        public bool Equals(BitmapStyle bitmapStyle)
        {
            if (!base.Equals(bitmapStyle))
                return false;

            if (BitmapId != bitmapStyle.BitmapId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return
                BitmapId.GetHashCode() ^
                base.GetHashCode();
        }

        public static bool operator ==(BitmapStyle bitmapStyle1, BitmapStyle bitmapStyle2)
        {
            return Equals(bitmapStyle1, bitmapStyle2);
        }

        public static bool operator !=(BitmapStyle bitmapStyle1, BitmapStyle bitmapStyle2)
        {
            return !Equals(bitmapStyle1, bitmapStyle2);
        }
    }
}