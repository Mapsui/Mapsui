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
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Color = System.Drawing.Color;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Gdi
{
    public class GdiMapRenderer : IRenderer
    {
        public Graphics Graphics { get; set; }
        
        static GdiMapRenderer()
        {
            DefaultRendererFactory.Create = () => new GdiMapRenderer();
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(Graphics, viewport, layers);
        }

        private static void Render(Graphics graphics, IViewport viewport, IEnumerable<ILayer> layers)
        {
            foreach (var layer in layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= viewport.Resolution &&
                    layer.MaxVisible >= viewport.Resolution)
                {
                    if (layer is LabelLayer)
                    {
                        GdiLabelRenderer.Render(graphics, viewport, layer as LabelLayer);
                    }
                    else
                    {
                        RenderLayer(graphics, viewport, layer);
                    }
                }
            }
        }

        public static Image RenderMapAsImage(IViewport viewport, IEnumerable<ILayer> layers)
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
            Image image = RenderMapAsImage(viewport, layers);
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            return memoryStream;
        }

        public byte[] RenderMapAsByteArray(IViewport viewport, IEnumerable<ILayer> layers)
        {
            return RenderToBitmapStream(viewport, layers).ToArray();
        }

        private static void RenderLayer(Graphics graphics, IViewport viewport, ILayer layer)
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
 
            var layerStyles = BaseLayer.GetLayerStyles(layer);
            
            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle;

                var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution);

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                var enumerable = features as IList<IFeature> ?? features.ToList();
                foreach (var feature in enumerable)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style is VectorStyle) && ((style as VectorStyle).Outline != null))
                    {
                        GdiGeometryRenderer.RenderGeometryOutline(graphics, viewport, feature.Geometry, style as VectorStyle);
                    }
                }

                foreach (var feature in enumerable)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    RenderGeometry(graphics, viewport, feature, style as VectorStyle);
                }
            }
        }

        private static void RenderGeometry(Graphics graphics, IViewport viewport, IFeature feature, VectorStyle style)
        {
            if (feature.Geometry is Point)
                GdiGeometryRenderer.DrawPoint(graphics, (Point)feature.Geometry, style, viewport);
            else if (feature.Geometry is MultiPoint)
                GdiGeometryRenderer.DrawMultiPoint(graphics, (MultiPoint) feature.Geometry, style, viewport);
            else if (feature.Geometry is LineString)
                GdiGeometryRenderer.DrawLineString(graphics, (LineString)feature.Geometry, style.Line.Convert(), viewport);
            else if (feature.Geometry is MultiLineString)
                GdiGeometryRenderer.DrawMultiLineString(graphics, (MultiLineString)feature.Geometry, style.Line.Convert(), viewport);
            else if (feature.Geometry is Polygon)
                GdiGeometryRenderer.DrawPolygon(graphics, (Polygon)feature.Geometry, style.Fill.Convert(), style.Outline.Convert(), viewport);
            else if (feature.Geometry is MultiPolygon)
                GdiGeometryRenderer.DrawMultiPolygon(graphics, (MultiPolygon)feature.Geometry, style.Fill.Convert(), style.Outline.Convert(), viewport);
            else if (feature.Geometry is IRaster)
                GdiGeometryRenderer.DrawRaster(graphics, feature.Geometry, style, viewport);
        }
    }
}