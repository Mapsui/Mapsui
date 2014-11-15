// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Copyright 2010 - Paul den Dulk (Geodan) - Adapted SharpMap for Mapsui.
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering.Gdi.Extensions;
using Mapsui.Styles;
using Color = System.Drawing.Color;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Gdi
{
    public class MapRenderer : IRenderer
    {
        public Graphics Graphics { get; set; }
        
        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(Graphics, viewport, layers);
        }

        private void Render(Graphics graphics, IViewport viewport, IEnumerable<ILayer> layers)
        {
            Graphics = graphics;
            VisibleFeatureIterator.IterateLayers(graphics, viewport, layers, RenderFeature);
        }

        public Image RenderMapAsImage(IViewport viewport, IEnumerable<ILayer> layers)
        {
            if ((viewport.Width <= 0) || (viewport.Height <= 0)) throw new Exception("The view's width or heigh is 0");
            var image = new System.Drawing.Bitmap((int)viewport.Width, (int)viewport.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(image);
            graphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, image.Width, image.Height);
            graphics.PageUnit = GraphicsUnit.Pixel;
            Render(graphics, viewport, layers);
            return image;
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            var image = RenderMapAsImage(viewport, layers);
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            return memoryStream;
        }

        public byte[] RenderMapAsByteArray(IViewport viewport, IEnumerable<ILayer> layers)
        {
            return RenderToBitmapStream(viewport, layers).ToArray();
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            var vectorStyle = style as VectorStyle;
            if (feature.Geometry is Point)
                PointRenderer.Render(Graphics, (Point)feature.Geometry, vectorStyle, viewport);
            else if (feature.Geometry is MultiPoint)
                MultiPointRenderer.Render(Graphics, (MultiPoint)feature.Geometry, vectorStyle, viewport);
            else if (feature.Geometry is LineString)
                LineStringRenderer.Render(Graphics, (LineString)feature.Geometry, vectorStyle.Line.ToGdi(), viewport);
            else if (feature.Geometry is MultiLineString)
                MultiLineStringRenderer.Render(Graphics, (MultiLineString)feature.Geometry, vectorStyle.Line.ToGdi(), viewport);
            else if (feature.Geometry is Polygon)
                PolygonRenderer.DrawPolygon(Graphics, (Polygon)feature.Geometry, vectorStyle.Fill.ToGdi(), vectorStyle.Outline.ToGdi(), viewport);
            else if (feature.Geometry is MultiPolygon)
                MultiPolygonRenderer.Render(Graphics, (MultiPolygon)feature.Geometry, vectorStyle.Fill.ToGdi(), vectorStyle.Outline.ToGdi(), viewport);
            else if (feature.Geometry is IRaster)
                RasterRenderer.Render(Graphics, feature.Geometry, vectorStyle, viewport);
        }
    }
}