
namespace SharpMap.Styles
{
    public class VectorStyle : Style
    {
        public VectorStyle()
        {
            Outline = new Pen { Color = Color.Gray, Width = 1 };
            Line = new Pen { Color = Color.Black, Width = 1 };
            Fill = new Brush { Color = Color.White };
        }
        /// <summary>
        /// Linestyle for line geometries
        /// </summary>
        public Pen Line { get; set; }

        /// <summary>
        /// Outline style for line and polygon geometries
        /// </summary>
        public Pen Outline { get; set; }

        /// <summary>
        /// Fillstyle for Polygon geometries
        /// </summary>
        public Brush Fill { get; set; }

        #region Equals operator

        public override bool Equals(object obj)
        {
            if (!(obj is VectorStyle))
            {
                return false;
            }
            return Equals((VectorStyle)obj);
        }

        public bool Equals(VectorStyle vectorStyle)
        {
            if (!base.Equals(vectorStyle))
            {
                return false;
            }

            if (!Line.Equals(vectorStyle.Line))
            {
                return false;
            }

            if (!Outline.Equals(vectorStyle.Outline))
            {
                return false;
            }

            if (!Fill.Equals(vectorStyle.Fill))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Line.GetHashCode() ^ Outline.GetHashCode() ^ Fill.GetHashCode() ^ base.GetHashCode();
        }

        public static bool operator ==(VectorStyle vectorStyle1, VectorStyle vectorStyle2)
        {
            return Equals(vectorStyle1, vectorStyle2);
        }

        public static bool operator !=(VectorStyle vectorStyle1, VectorStyle vectorStyle2)
        {
            return !Equals(vectorStyle1, vectorStyle2);
        }

        #endregion

    }
}
