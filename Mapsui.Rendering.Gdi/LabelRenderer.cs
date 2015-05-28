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
using System.Drawing.Drawing2D;
using Mapsui.Rendering.Gdi.Extensions;
using Mapsui.Styles;
using Mapsui.Utilities;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Gdi
{
    class LabelRenderer
    {
        /// <summary>
        /// Renders a label to the map.
        /// </summary>
        /// <param name="graphics">Graphics reference</param>
        /// <param name="labelPoint">Label placement</param>
        /// <param name="offset">Offset of label in screen coordinates</param>
        /// <param name="font">Font used for rendering</param>
        /// <param name="forecolor">Font forecolor</param>
        /// <param name="backcolor">Background color</param>
        /// <param name="halo">Color of halo</param>
        /// <param name="rotation">Text rotation in degrees</param>
        /// <param name="text">Text to render</param>
        /// <param name="viewport"></param>
        public static void DrawLabel(Graphics graphics, Point labelPoint, Offset offset, Styles.Font font, Styles.Color forecolor, Styles.Brush backcolor, Styles.Pen halo, double rotation, string text, IViewport viewport)
        {
            SizeF fontSize = graphics.MeasureString(text, font.ToGdi()); //Calculate the size of the text
            labelPoint.X += offset.X; labelPoint.Y += offset.Y; //add label offset
            if (Math.Abs(rotation) > Constants.Epsilon && !double.IsNaN(rotation))
            {
                graphics.TranslateTransform((float)labelPoint.X, (float)labelPoint.Y);
                graphics.RotateTransform((float)rotation);
                graphics.TranslateTransform(-fontSize.Width / 2, -fontSize.Height / 2);
                if (backcolor != null && backcolor.ToGdi() != Brushes.Transparent)
                    graphics.FillRectangle(backcolor.ToGdi(), 0, 0, fontSize.Width * 0.74f + 1f, fontSize.Height * 0.74f);
                var path = new GraphicsPath();
                path.AddString(text, new FontFamily(font.FontFamily), (int)font.ToGdi().Style, font.ToGdi().Size, new System.Drawing.Point(0, 0), null);
                if (halo != null)
                    graphics.DrawPath(halo.ToGdi(), path);
                graphics.FillPath(new SolidBrush(forecolor.ToGdi()), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), 0, 0);                
            }
            else
            {
                if (backcolor != null && backcolor.ToGdi() != Brushes.Transparent)
                    graphics.FillRectangle(backcolor.ToGdi(), (float)labelPoint.X, (float)labelPoint.Y, fontSize.Width * 0.74f + 1, fontSize.Height * 0.74f);

                var path = new GraphicsPath();

                //Arial hack
                path.AddString(text, new FontFamily("Arial"), (int)font.ToGdi().Style, (float)font.Size, new System.Drawing.Point((int)labelPoint.X, (int)labelPoint.Y), null);
                if (halo != null)
                    graphics.DrawPath(halo.ToGdi(), path);
                graphics.FillPath(new SolidBrush(forecolor.ToGdi()), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), LabelPoint.X, LabelPoint.Y);
            }
        }
    }
}
