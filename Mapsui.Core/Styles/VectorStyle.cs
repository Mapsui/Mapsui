// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
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
        /// Line style for line geometries
        /// </summary>
        public Pen? Line { get; set; }

        /// <summary>
        /// Outline style for line and polygon geometries
        /// </summary>
        public Pen? Outline { get; set; }

        /// <summary>
        /// Fill style for Polygon geometries
        /// </summary>
        public Brush? Fill { get; set; }

        public override bool Equals(object? obj)
        {
            if (!(obj is VectorStyle style))
            {
                return false;
            }
            return Equals(style);
        }

        public bool Equals(VectorStyle vectorStyle)
        {
            if (!base.Equals(vectorStyle))
            {
                return false;
            }

            if ((Line is null) ^ (vectorStyle.Line is null))
            {
                return false;
            }

            if (Line is not null && !Line.Equals(vectorStyle.Line))
            {
                return false;
            }

            if ((Outline is null) ^ (vectorStyle.Outline is null))
            {
                return false;
            }

            if (Outline is not null && !Outline.Equals(vectorStyle.Outline))
            {
                return false;
            }

            if ((Fill is null) ^ (vectorStyle.Fill is null))
            {
                return false;
            }

            if (Fill is not null && !Fill.Equals(vectorStyle.Fill))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (Line is null ? 0 : Line.GetHashCode())
                ^ (Outline is null ? 0 : Outline.GetHashCode())
                ^ (Fill is null ? 0 : Fill.GetHashCode())
                ^ base.GetHashCode();
        }

        public static bool operator ==(VectorStyle vectorStyle1, VectorStyle vectorStyle2)
        {
            return Equals(vectorStyle1, vectorStyle2);
        }

        public static bool operator !=(VectorStyle vectorStyle1, VectorStyle vectorStyle2)
        {
            return !Equals(vectorStyle1, vectorStyle2);
        }
    }
}
