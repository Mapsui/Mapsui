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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SharpMap.Styles;
using SharpMap.Styles.Thematics;
using Point = SharpMap.Geometries.Point;

namespace GdiRendering
{
    public class GdiMapRenderer
    {
        // TODO: derive from IRenderer
        public delegate bool AbortRenderDelegate();

        public static void Render(Graphics graphics, IView view, Map map, AbortRenderDelegate abortRender)
        {
            foreach (var layer in map.Layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= view.Resolution &&
                    layer.MaxVisible >= view.Resolution)
                {
                    if (layer is LabelLayer)
                    {
                        //!!!GdiLabelRenderer.Render(graphics, view, layer as LabelLayer);
                    }
                    else if (layer is ITileLayer)
                    {
                        var tileLayer = (layer as ITileLayer);
                        GdiTileRenderer.Render(graphics, tileLayer.Schema, view, tileLayer.MemoryCache);
                    }
                    else
                    {
                        RenderLayer(graphics, view, layer, abortRender);
                    }
                }
                
                if (abortRender != null && abortRender()) return; 
            }
        }

        public static Image RenderMapAsImage(IView view, Map map)
        {
            if ((view.Width <= 0) || (view.Height <= 0)) throw new Exception("The view's width or heigh is 0");
            var image = new System.Drawing.Bitmap((int)view.Width, (int)view.Height);
            var graphics = Graphics.FromImage(image);
#if !PocketPC
            graphics.PageUnit = GraphicsUnit.Pixel;
#endif
            Render(graphics, view, map, null);
            return image;
        }

        public byte[] RenderMapAsByteArray(IView view, Map map)
        {
            Image image = RenderMapAsImage(view, map);
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Bmp);
            return memoryStream.ToArray();
        }

        private static void RenderLayer(Graphics graphics, IView view, ILayer layer, AbortRenderDelegate abortRender)
        {
            int counter = 0;
            const int step = 100;

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                var features = layer.GetFeaturesInView(view.Extent, view.Resolution);

                //Linestring outlines is drawn by drawing the layer once with a thicker line
                //before drawing the "inline" on top.
                foreach (IFeature feature in features)
                {
                    if ((counter++ % step == 0) && abortRender != null && abortRender()) return;
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style is VectorStyle) && ((style as VectorStyle).Outline != null))
                    {
                        GdiGeometryRenderer.RenderGeometryOutline(graphics, view, feature.Geometry, style as VectorStyle);
                    }
                }

                foreach (IFeature feature in features)
                {
                    if ((counter++ % step == 0) && abortRender != null && abortRender()) return;
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    RenderGeometry(graphics, view, feature.Geometry, style as VectorStyle);
                }
            }
        }

        private static void RenderGeometry(Graphics graphics, IView transform, IGeometry feature, VectorStyle style)
        {
            if (feature is Point)
                GdiGeometryRenderer.DrawPoint(graphics, (Point)feature, style, transform);
            else if (feature is MultiPoint)
                GdiGeometryRenderer.DrawMultiPoint(graphics, (MultiPoint) feature, style, transform);
            else if (feature is LineString)
                GdiGeometryRenderer.DrawLineString(graphics, (LineString)feature, style.Line.Convert(), transform);
            else if (feature is MultiLineString)
                GdiGeometryRenderer.DrawMultiLineString(graphics, (MultiLineString)feature, style.Line.Convert(), transform);
            else if (feature is Polygon)
                GdiGeometryRenderer.DrawPolygon(graphics, (Polygon)feature, style.Fill.Convert(), style.Outline.Convert(), transform);
            else if (feature is MultiPolygon)
                GdiGeometryRenderer.DrawMultiPolygon(graphics, (MultiPolygon)feature, style.Fill.Convert(), style.Outline.Convert(), transform);
            else if (feature is IRaster)
                GdiGeometryRenderer.DrawRaster(graphics, feature as IRaster, transform);
        }
    }
}