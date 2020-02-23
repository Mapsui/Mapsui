using System;
using Mapsui.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
{
    public enum SymbolType
    {
        Ellipse,
        Rectangle,
        Triangle
    }

    public class SymbolStyle : ImageStyle // todo: derive SymbolStyle from VectorStyle after v2.
    {
        public SymbolStyle() : base() { }

        public static double DefaultWidth { get; set; } = 32;
        public static double DefaultHeight { get; set; } = 32;
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