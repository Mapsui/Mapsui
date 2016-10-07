// Copyright 20010 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using Mapsui.Styles;

namespace Mapsui.Rendering.Gdi.Extensions
{
    public static class StyleExtensions
    {
        public static System.Drawing.Color ToGdi(this Color color)
        {
            if (color == null) return System.Drawing.Color.Transparent;
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Pen ToGdi(this Pen pen)
        {
            return new System.Drawing.Pen(pen.Color.ToGdi(), (float)pen.Width);
        }

        public static System.Drawing.Brush ToGdi(this Brush brush)
        {
            return new System.Drawing.SolidBrush(brush.Color.ToGdi());
        }

        public static System.Drawing.PointF ToGdi(this Offset offset)
        {
            return new System.Drawing.PointF((float)offset.X, (float)offset.Y);
        }

        public static System.Drawing.Font ToGdi(this Font font)
        {
            return new System.Drawing.Font(font.FontFamily, (float)font.Size);
        }
    }
}
