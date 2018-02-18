// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
{
    /// <summary>
    ///     Defines a style used for rendering vector data
    /// </summary>
    public class Style : IStyle
    {
        public Style()
        {
            MinVisible = 0;
            MaxVisible = double.MaxValue;
            Enabled = true;
            Opacity = 1f;
        }

        /// <summary>
        ///     Gets or sets the minimum zoom value where the style is applied
        /// </summary>
        public double MinVisible { get; set; }

        /// <summary>
        ///     Gets or sets the maximum zoom value where the style is applied
        /// </summary>
        public double MaxVisible { get; set; }

        /// <summary>
        ///     Gets or sets whether objects in this style is rendered or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the objects base opacity
        /// </summary>
        public float Opacity { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Style))
                return false;
            return Equals((Style) obj);
        }

        public bool Equals(Style style)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (MinVisible != style.MinVisible)
                return false;

            if (MaxVisible != style.MaxVisible)
                return false;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            if (Enabled != style.Enabled)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return MinVisible.GetHashCode() ^ MaxVisible.GetHashCode() ^ Enabled.GetHashCode() ^ Opacity.GetHashCode();
        }

        public static bool operator ==(Style style1, Style style2)
        {
            return Equals(style1, style2);
        }

        public static bool operator !=(Style style1, Style style2)
        {
            return !Equals(style1, style2);
        }
    }
}