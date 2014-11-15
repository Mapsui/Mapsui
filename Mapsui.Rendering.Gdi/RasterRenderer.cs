// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2010 - Paul den Dulk (Geodan) - Adapted SharpMap for Mapsui
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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.Utilities;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;

namespace Mapsui.Rendering.Gdi
{
    class RasterRenderer
    {
        public static void Render(Graphics graphics, IGeometry feature, IStyle style, IViewport viewport)
        {
            var stream = ((IRaster)feature).Data;
            stream.Position = 0;
            var bitmap = new Bitmap(stream);

            Geometries.Point min = viewport.WorldToScreen(new Geometries.Point(feature.GetBoundingBox().MinX, feature.GetBoundingBox().MinY));
            Geometries.Point max = viewport.WorldToScreen(new Geometries.Point(feature.GetBoundingBox().MaxX, feature.GetBoundingBox().MaxY));

            Rectangle destination = RoundToPixel(new RectangleF((float)min.X, (float)max.Y, (float)(max.X - min.X), (float)(min.Y - max.Y)));
            graphics.DrawImage(bitmap,
                destination,
                0, 0, bitmap.Width, bitmap.Height,
                GraphicsUnit.Pixel,
                new ImageAttributes());

            if (DeveloperTools.DeveloperMode)
            {
                var font = new System.Drawing.Font("Arial", 12);
                var message = (GC.GetTotalMemory(true) / 1000).ToString(CultureInfo.InvariantCulture) + " KB";
                graphics.DrawString(message, font, new SolidBrush(Color.Black), 10f, 10f);
            }

            bitmap.Dispose();
        }

        private static Rectangle RoundToPixel(RectangleF dest)
        {
            // To get seamless aligning you need to round the locations
            // not the width and height
            var result = new Rectangle(
                (int)Math.Round(dest.Left),
                (int)Math.Round(dest.Top),
                (int)(Math.Round(dest.Right) - Math.Round(dest.Left)),
                (int)(Math.Round(dest.Bottom) - Math.Round(dest.Top)));
            return result;
        }
    }
}
