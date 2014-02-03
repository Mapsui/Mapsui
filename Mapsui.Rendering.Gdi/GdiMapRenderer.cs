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
using System.Drawing.Drawing2D;
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

        public GdiMapRenderer()
        {
            RendererFactory.Get = () => this;
        }

        public delegate bool AbortRenderDelegate();

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(Graphics, viewport, layers, null);
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers, AbortRenderDelegate abortRender)
        {
            Render(Graphics, viewport, layers, abortRender);
        }

        private static void Render(Graphics graphics, IViewport viewport, IEnumerable<ILayer> layers, AbortRenderDelegate abortRender)
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
                    else if (layer is ITileLayer)
                    {
                        var tileLayer = (layer as ITileLayer);
                        GdiTileRenderer.Render(graphics, tileLayer.Schema, viewport, tileLayer.MemoryCache);
                    }
                    else
                    {
                        RenderLayer(graphics, viewport, layer, abortRender);
                    }
                }
                
                if (abortRender != null && abortRender()) return; 
            }
        }

        public static Image RenderMapAsImage(IViewport viewport, IEnumerable<ILayer> layers)
        {
            if ((viewport.Width <= 0) || (viewport.Height <= 0)) throw new Exception("The view's width or heigh is 0");
            var image = new System.Drawing.Bitmap((int)viewport.Width, (int)viewport.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(image);
            graphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, image.Width, image.Height);
            graphics.PageUnit = GraphicsUnit.Pixel;
            Render(graphics, viewport, layers, null);
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

        private static void RenderLayer(Graphics graphics, IViewport viewport, ILayer layer, AbortRenderDelegate abortRender)
        {
            int counter = 0;
            const int step = 100;

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
                    if ((counter++ % step == 0) && abortRender != null && abortRender()) return;
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style is VectorStyle) && ((style as VectorStyle).Outline != null))
                    {
                        GdiGeometryRenderer.RenderGeometryOutline(graphics, viewport, feature.Geometry, style as VectorStyle);
                    }
                }

                foreach (var feature in enumerable)
                {
                    if ((counter++ % step == 0) && abortRender != null && abortRender()) return;
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    RenderGeometry(graphics, viewport, feature.Geometry, style as VectorStyle);
                }
            }
        }

        private static void RenderGeometry(Graphics graphics, IViewport viewport, IGeometry geometry, VectorStyle style)
        {
            if (geometry is Point)
                GdiGeometryRenderer.DrawPoint(graphics, (Point)geometry, style, viewport);
            else if (geometry is MultiPoint)
                GdiGeometryRenderer.DrawMultiPoint(graphics, (MultiPoint) geometry, style, viewport);
            else if (geometry is LineString)
                GdiGeometryRenderer.DrawLineString(graphics, (LineString)geometry, style.Line.Convert(), viewport);
            else if (geometry is MultiLineString)
                GdiGeometryRenderer.DrawMultiLineString(graphics, (MultiLineString)geometry, style.Line.Convert(), viewport);
            else if (geometry is Polygon)
                GdiGeometryRenderer.DrawPolygon(graphics, (Polygon)geometry, style.Fill.Convert(), style.Outline.Convert(), viewport);
            else if (geometry is MultiPolygon)
                GdiGeometryRenderer.DrawMultiPolygon(graphics, (MultiPolygon)geometry, style.Fill.Convert(), style.Outline.Convert(), viewport);
            else if (geometry is IRaster)
                GdiGeometryRenderer.DrawRaster(graphics, geometry as IRaster, viewport);
        }
    }
}