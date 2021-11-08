// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
{
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
            if (!(obj is Brush brush))
            {
                return false;
            }
            return Equals(brush);
        }

        public bool Equals(Brush? brush)
        {
            if (brush is null)
                return false;

            if ((Color is null) ^ (brush.Color is null))
            {
                return false;
            }

            if (Color != brush.Color)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Color is null ? 0 : Color.GetHashCode();
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
}
