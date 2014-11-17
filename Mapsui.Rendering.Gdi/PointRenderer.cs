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
using Mapsui.Rendering.Gdi.Extensions;
using Mapsui.Styles;
using Bitmap = System.Drawing.Bitmap;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Gdi
{
    static class PointRenderer
    {
        public static void Render(Graphics graphics, Point point, IStyle style, IViewport viewport)
        {
            if (point == null) return;
            if (style == null) return;

            var symbolStyle = style as SymbolStyle;
            if ((symbolStyle == null) || (symbolStyle.BitmapId < 0))
            {
                RenderVectorPoint(graphics, point, style, viewport);
            }
            else
            {
                RenderBitmapPoint(graphics, point, viewport, symbolStyle);
            }
        }

        private static void RenderBitmapPoint(Graphics graphics, Point point, IViewport viewport, SymbolStyle symbolStyle)
        {
            var symbol = new Bitmap(BitmapRegistry.Instance.Get(symbolStyle.BitmapId));
            var symbolscale = symbolStyle.SymbolScale;
            var offset = symbolStyle.SymbolOffset.ToGdi();
            var rotation = symbolStyle.SymbolRotation;
            var dest = ConvertPoint(viewport.WorldToScreen(point));

            var width = symbol.Width * symbolscale;
            var height = symbol.Height * symbolscale;

            graphics.TranslateTransform(dest.X, dest.Y);
            graphics.RotateTransform((float)rotation);
            graphics.TranslateTransform(offset.X, -offset.Y);
            graphics.TranslateTransform((int)(-width / 2.0), (int)(-height / 2.0));

            graphics.DrawImage(symbol, 0, 0, (float)width, (float)height);
            graphics.ResetTransform();
        }

        private static void RenderVectorPoint(Graphics graphics, Point point, IStyle style, IViewport viewport)
        {
            var symbolStyle = ToSymbolStyle(style);
            if (symbolStyle.Fill == null) return;

            var symbolscale = symbolStyle.SymbolScale;
            var offset = symbolStyle.SymbolOffset.ToGdi();
            var rotation = symbolStyle.SymbolRotation;
            var dest = ConvertPoint(viewport.WorldToScreen(point));

            var width = SymbolStyle.DefaultWidth * symbolscale;
            var height = SymbolStyle.DefaultHeight * symbolscale;

            graphics.TranslateTransform(dest.X, dest.Y);
            graphics.RotateTransform((float)rotation);
            graphics.TranslateTransform(offset.X, -offset.Y);
            graphics.TranslateTransform((int)(-width / 2.0), (int)(-height / 2.0));

            DrawSymbol(graphics, symbolStyle);
            graphics.ResetTransform();
        }

        private static void DrawSymbol(Graphics graphics, SymbolStyle symbolStyle)
        {
            using (var fill = symbolStyle.Fill.ToGdi())
            {
                if (symbolStyle.SymbolType == SymbolType.Rectangle)
                {
                    graphics.FillRectangle(fill, 0, 0, (int)SymbolStyle.DefaultWidth, (int)SymbolStyle.DefaultHeight);
                }
                else
                {
                    graphics.FillEllipse(fill, 0, 0, (int)SymbolStyle.DefaultWidth, (int)SymbolStyle.DefaultHeight);
                }
            }
        }

        private static SymbolStyle ToSymbolStyle(IStyle style)
        {
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null) return symbolStyle;
            var vectorStyle = style as VectorStyle;
            if (vectorStyle == null)
                symbolStyle = new SymbolStyle();
            else
                symbolStyle = new SymbolStyle
                {
                    Fill = vectorStyle.Fill,
                    Outline = vectorStyle.Outline,
                    Line = vectorStyle.Line
                };
            return symbolStyle;
        }

        public static PointF ConvertPoint(Point point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }
    }
}
