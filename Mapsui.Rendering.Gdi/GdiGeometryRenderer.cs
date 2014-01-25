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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Mapsui.Geometries;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Gdi
{
    /// <summary>
    /// This class renders individual geometry features to a graphics object using the settings of a map object.
    /// </summary>
    public static class GdiGeometryRenderer
    {
        private static readonly Bitmap DefaultSymbol;

        static GdiGeometryRenderer()
        {
            DefaultSymbol = CreateDefaultSymbol();
        }

        private static Bitmap CreateDefaultSymbol()
        {
            const int size = 16;
            var bitmap = new Bitmap(size, size);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, size, size);
            graphics.FillEllipse(new SolidBrush(Color.Black), 0, 0, size - 1, size - 1);
            return bitmap;
        }

        public static void RenderGeometryOutline(Graphics graphics, IViewport viewport, IGeometry geometry, Styles.VectorStyle style)
        {
            //Draw background of all line-outlines first
            if (geometry is LineString)
            {
                DrawLineString(graphics, geometry as LineString, style.Outline.Convert(), viewport);
            }
            else if (geometry is MultiLineString)
            {
                DrawMultiLineString(graphics, geometry as MultiLineString, style.Outline.Convert(), viewport);
            }
        }

        public static void DrawMultiPoint(Graphics graphics, MultiPoint points, Styles.IStyle style, IViewport viewport)
        {
            foreach (Point point in points) DrawPoint(graphics, point, style, viewport);
        }

        public static void DrawMultiLineString(Graphics graphics, MultiLineString lines, Pen pen, IViewport viewport)
        {
            foreach (LineString t in lines.LineStrings)
                DrawLineString(graphics, t, pen, viewport);
        }

        public static void DrawLineString(Graphics graphics, LineString line, Pen pen, IViewport viewport)
        {
            if (line.Vertices.Count > 1)
            {
                var gp = new GraphicsPath();
                gp.AddLines(ConvertPoints(WorldToView(line, viewport)));
                graphics.DrawPath(pen, gp);
            }
        }

        private static PointF[] ConvertPoints(IEnumerable<Point> points)
        {
            var result = new List<PointF>();
            foreach (Point point in points) result.Add(new PointF((float)point.X, (float)point.Y));
            return result.ToArray();
        }

        public static void DrawMultiPolygon(Graphics graphics, MultiPolygon pols, Brush brush, Pen pen, IViewport viewport)
        {
            foreach (Polygon t in pols.Polygons)
                DrawPolygon(graphics, t, brush, pen, viewport);
        }

        public static void DrawPolygon(Graphics graphics, Polygon pol, Brush brush, Pen pen, IViewport viewport)
        {
            if (pol.ExteriorRing == null)
                return;
            if (pol.ExteriorRing.Vertices.Count > 2)
            {
                //Use a graphics path instead of DrawPolygon. DrawPolygon has a problem with several interior holes
                var gp = new GraphicsPath();

                //Add the exterior polygon
                gp.AddPolygon(ConvertPoints(WorldToView(pol.ExteriorRing, viewport)));
                //Add the interior polygons (holes)
                foreach (LinearRing linearRing in pol.InteriorRings)
                    gp.AddPolygon(ConvertPoints(WorldToView(linearRing, viewport)));

                // Only render inside of polygon if the brush isn't null or isn't transparent
                if (brush != null && brush != Brushes.Transparent)
                    graphics.FillPath(brush, gp);
                // Create an outline if a pen style is available
                if (pen != null)
                    graphics.DrawPath(pen, gp);
            }
        }

        public static IEnumerable<Point> WorldToView(LineString linearRing, IViewport viewport)
        {
            var v = new Point[linearRing.Vertices.Count];
            for (int i = 0; i < linearRing.Vertices.Count; i++)
                v[i] = viewport.WorldToScreen(linearRing.Vertices[i]);
            return v;
        }

        public static Point WorldToView(Point point, IViewport viewport)
        {
            return viewport.WorldToScreen(point);
        }

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
        public static void DrawLabel(Graphics graphics, Point labelPoint, Styles.Offset offset, Styles.Font font, Styles.Color forecolor, Styles.Brush backcolor, Styles.Pen halo, double rotation, string text, IViewport viewport)
        {
            SizeF fontSize = graphics.MeasureString(text, font.Convert()); //Calculate the size of the text
            labelPoint.X += offset.X; labelPoint.Y += offset.Y; //add label offset
            if (rotation != 0 && !double.IsNaN(rotation))
            {
                graphics.TranslateTransform((float)labelPoint.X, (float)labelPoint.Y);
                graphics.RotateTransform((float)rotation);
                graphics.TranslateTransform(-fontSize.Width / 2, -fontSize.Height / 2);
                if (backcolor != null && backcolor.Convert() != Brushes.Transparent)
                    graphics.FillRectangle(backcolor.Convert(), 0, 0, fontSize.Width * 0.74f + 1f, fontSize.Height * 0.74f);
                var path = new GraphicsPath();
                path.AddString(text, new FontFamily(font.FontFamily), (int)font.Convert().Style, font.Convert().Size, new System.Drawing.Point(0, 0), null);
                if (halo != null)
                    graphics.DrawPath(halo.Convert(), path);
                graphics.FillPath(new SolidBrush(forecolor.Convert()), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), 0, 0);                
            }
            else
            {
                if (backcolor != null && backcolor.Convert() != Brushes.Transparent)
                    graphics.FillRectangle(backcolor.Convert(), (float)labelPoint.X, (float)labelPoint.Y, fontSize.Width * 0.74f + 1, fontSize.Height * 0.74f);

                var path = new GraphicsPath();

                //Arial hack
                path.AddString(text, new FontFamily("Arial"), (int)font.Convert().Style, (float)font.Size, new System.Drawing.Point((int)labelPoint.X, (int)labelPoint.Y), null);
                if (halo != null)
                    graphics.DrawPath(halo.Convert(), path);
                graphics.FillPath(new SolidBrush(forecolor.Convert()), path);
                //g.DrawString(text, font, new System.Drawing.SolidBrush(forecolor), LabelPoint.X, LabelPoint.Y);
            }
        }
        
        public static void DrawPoint(Graphics graphics, Point point, Styles.IStyle style, IViewport viewport)
        {
            var vectorStyle = (Styles.SymbolStyle)style;
            if (vectorStyle.Symbol == null) throw  new ArgumentException("No bitmap symbol set in Gdi rendering"); //todo: allow vector symbol
            Bitmap symbol= vectorStyle.Symbol.Convert();
            var symbolscale = vectorStyle.SymbolScale;
            PointF offset = vectorStyle.SymbolOffset.Convert();
            var rotation = vectorStyle.SymbolRotation;

            if (point == null)
                return;
            if (symbol == null)
                symbol = DefaultSymbol;

            PointF dest = ConvertPoint(viewport.WorldToScreen(point));

            if (rotation != 0 && !double.IsNaN(rotation))
            {
                graphics.TranslateTransform(dest.X, dest.Y);
                graphics.RotateTransform((float)rotation);
                graphics.TranslateTransform((int)(-symbol.Width / 2.0), (int)(-symbol.Height / 2.0));
                if (symbolscale == 1f)
                    graphics.DrawImageUnscaled(symbol, (int)(dest.X - symbol.Width / 2.0 + offset.X), (int)(dest.Y - symbol.Height / 2.0 + offset.Y));
                else
                {
                    var width = symbol.Width * symbolscale;
                    var height = symbol.Height * symbolscale;
                    graphics.DrawImage(symbol, (int)(dest.X - width / 2 + offset.X * symbolscale), (int)(dest.Y - height / 2 + offset.Y * symbolscale), (float)width, (float)height);
                }
            }
            else
            {
                if (symbolscale == 1f)
                    graphics.DrawImageUnscaled(symbol, (int)(dest.X - symbol.Width / 2.0 + offset.X), (int)(dest.Y - symbol.Height / 2.0 + offset.Y));
                else
                {
                    var width = symbol.Width * symbolscale;
                    var height = symbol.Height * symbolscale;
                    graphics.DrawImage(symbol, (int)(dest.X - width / 2 + offset.X * symbolscale), (int)(dest.Y - height / 2 + offset.Y * symbolscale), (float)width, (float)height);
                }
            }
        }
        
        public static PointF ConvertPoint(Point point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }

        public static void DrawRaster(Graphics graphics, IRaster raster, IViewport viewport)
        {
            var imageAttributes = new ImageAttributes();

            var bitmap = new Bitmap(raster.Data);

            Point min = viewport.WorldToScreen(new Point(raster.GetBoundingBox().MinX, raster.GetBoundingBox().MinY));
            Point max = viewport.WorldToScreen(new Point(raster.GetBoundingBox().MaxX, raster.GetBoundingBox().MaxY));

            Rectangle destination = RoundToPixel(new RectangleF((float)min.X, (float)max.Y, (float)(max.X - min.X), (float)(min.Y - max.Y)));
            graphics.DrawImage(bitmap,
                destination,
                0, 0, bitmap.Width, bitmap.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
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
