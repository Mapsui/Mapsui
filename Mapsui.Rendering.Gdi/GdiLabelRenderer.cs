// Copyright 20010 - Paul den Dulk (Geodan) - Adapted SharpMap for Mapsui.
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
using System.Globalization;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Gdi
{
    public static class GdiLabelRenderer
    {
        public static void Render(Graphics g, IViewport viewport, LabelLayer labelLayer)
        {
            var layerStyles = BaseLayer.GetLayerStyles(labelLayer);
            foreach (var layerStyle in layerStyles)
            {
                if (layerStyle.Enabled && labelLayer.MaxVisible >= viewport.Resolution && labelLayer.MinVisible < viewport.Resolution)
                {
                    if (labelLayer.DataSource == null)
                        throw (new ApplicationException("DataSource property not set"));

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    var features = labelLayer.GetFeaturesInView(viewport.Extent, viewport.Resolution);

                    //Initialize label collection
                    var labels = new List<Label>();

                    var style = layerStyle as LabelStyle;

                    //List<System.Drawing.Rectangle> LabelBoxes; //Used for collision detection
                    //Render labels
                    foreach (IFeature feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature) as LabelStyle;

                        float rotation = 0;
                        if (!String.IsNullOrEmpty(labelLayer.RotationColumn))
                            rotation = float.Parse(feature[labelLayer.RotationColumn].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);

                        int priority = labelLayer.Priority;
                        if (labelLayer.PriorityDelegate != null)
                            priority = labelLayer.PriorityDelegate(feature);
                        else if (!String.IsNullOrEmpty(labelLayer.PriorityColumn))
                            priority = int.Parse(feature[labelLayer.PriorityColumn].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);

                        string text;
                        if (labelLayer.LabelStringDelegate != null)
                        {
                            text = labelLayer.LabelStringDelegate(feature);
                        }
                        else
                        {
                            text = feature[labelLayer.LabelColumn].ToString();
                        }

                        if (!string.IsNullOrEmpty(text))
                        {
                            if (feature.Geometry is GeometryCollection)
                            {
                                var geometryCollection = feature.Geometry as GeometryCollection;
                                if (labelLayer.MultipartGeometryBehaviour == LabelLayer.MultipartGeometryBehaviourEnum.All)
                                {
                                    foreach (var geometry in geometryCollection)
                                    {
                                        var label = CreateLabel(geometry, text, rotation, priority, style, viewport, g);
                                        if (label != null) labels.Add(label);
                                    }
                                }
                                else if (labelLayer.MultipartGeometryBehaviour == LabelLayer.MultipartGeometryBehaviourEnum.CommonCenter)
                                {
                                    var label = CreateLabel(feature.Geometry, text, rotation, priority, style, viewport, g);
                                    if (label != null) labels.Add(label);
                                }
                                else if (labelLayer.MultipartGeometryBehaviour == LabelLayer.MultipartGeometryBehaviourEnum.First)
                                {
                                    if ((feature.Geometry as GeometryCollection).Collection.Count > 0)
                                    {
                                        Label label = CreateLabel(geometryCollection.Collection[0], text, rotation, 0,
                                            style, viewport, g);
                                        if (label != null) labels.Add(label);
                                    }
                                }
                                else if (labelLayer.MultipartGeometryBehaviour == LabelLayer.MultipartGeometryBehaviourEnum.Largest)
                                {
                                    var coll = (feature.Geometry as GeometryCollection);
                                    if (coll.NumGeometries > 0)
                                    {
                                        double largestVal = 0;
                                        int idxOfLargest = 0;
                                        for (var j = 0; j < coll.NumGeometries; j++)
                                        {
                                            Geometry geom = coll.Geometry(j);
                                            if (geom is LineString && ((LineString)geom).Length > largestVal)
                                            {
                                                largestVal = ((LineString)geom).Length;
                                                idxOfLargest = j;
                                            }
                                            if (geom is MultiLineString && ((MultiLineString)geom).Length > largestVal)
                                            {
                                                largestVal = ((MultiLineString)geom).Length;
                                                idxOfLargest = j;
                                            }
                                            if (geom is Polygon && ((Polygon)geom).Area > largestVal)
                                            {
                                                largestVal = ((Polygon)geom).Area;
                                                idxOfLargest = j;
                                            }
                                            if (geom is MultiPolygon && ((MultiPolygon)geom).Area > largestVal)
                                            {
                                                largestVal = ((MultiPolygon)geom).Area;
                                                idxOfLargest = j;
                                            }
                                        }

                                        var label = CreateLabel(coll.Geometry(idxOfLargest), text, rotation, priority, style,
                                                                viewport, g);
                                        if (label != null) labels.Add(label);
                                    }
                                }
                            }
                            else
                            {
                                var label = CreateLabel(feature.Geometry, text, rotation, priority, style, viewport, g);
                                if (label != null) labels.Add(label);
                            }
                        }
                    }

                    if (labels.Count > 0) //We have labels to render...
                    {
                        if ((layerStyle is LabelStyle) && (layerStyle as LabelStyle).CollisionDetection && labelLayer.LabelFilter != null)
                            labelLayer.LabelFilter(labels);
                        foreach (Label label in labels)
                        {
                            if (!label.Show) continue;
                            GdiGeometryRenderer.DrawLabel(g, label.LabelPoint, label.Style.Offset, label.Style.Font,
                                label.Style.ForeColor, label.Style.BackColor, label.Halo, label.Rotation, label.Text, viewport);
                        }
                    }
                }
            }
        }

        private static Label CreateLabel(IGeometry feature, string text, float rotation, int priority, LabelStyle style, IViewport viewport,
                                  Graphics g)
        {
            var gdiSize = g.MeasureString(text, style.Font.ToBitmap());
            var size = new Styles.Size { Width = gdiSize.Width, Height = gdiSize.Height };

            Geometries.Point position = viewport.WorldToScreen(feature.GetBoundingBox().GetCentroid());
            position.X = position.X - size.Width * (short)style.HorizontalAlignment * 0.5f;
            position.Y = position.Y - size.Height * (short)style.VerticalAlignment * 0.5f;
            if (position.X - size.Width > viewport.Width || position.X + size.Width < 0 ||
                position.Y - size.Height > viewport.Height || position.Y + size.Height < 0)
                return null;

            Label lbl;

            if (!style.CollisionDetection)
                lbl = new Label(text, position, rotation, priority, null, style);
            else
            {
                //Collision detection is enabled so we need to measure the size of the string
                lbl = new Label(text, position, rotation, priority,
                                new LabelBox(position.X - size.Width * 0.5f - style.CollisionBuffer.Width,
                                             position.Y + size.Height * 0.5f + style.CollisionBuffer.Height,
                                             size.Width + 2f * style.CollisionBuffer.Width,
                                             size.Height + style.CollisionBuffer.Height * 2f), style);
            }
            if (feature is LineString)
            {
                var line = feature as LineString;
                if (line.Length / viewport.Resolution > size.Width) //Only label feature if it is long enough
                    CalculateLabelOnLinestring(line, ref lbl, viewport);
                else
                    return null;
            }

            return lbl;
        }

        private static void CalculateLabelOnLinestring(LineString line, ref Label label, IViewport viewportTransform)
        {
            double dx, dy;

            // first find the middle segment of the line
            int midPoint = (line.Vertices.Count - 1) / 2;
            if (line.Vertices.Count > 2)
            {
                dx = line.Vertices[midPoint + 1].X - line.Vertices[midPoint].X;
                dy = line.Vertices[midPoint + 1].Y - line.Vertices[midPoint].Y;
            }
            else
            {
                midPoint = 0;
                dx = line.Vertices[1].X - line.Vertices[0].X;
                dy = line.Vertices[1].Y - line.Vertices[0].Y;
            }
            if (Math.Abs(dy - 0) < Constants.Epsilon)
                label.Rotation = 0;
            else if (Math.Abs(dx - 0) < Constants.Epsilon)
                label.Rotation = 90;
            else
            {
                // calculate angle of line					
                double angle = -Math.Atan(dy / dx) + Math.PI * 0.5;
                angle *= (180d / Math.PI); // convert radians to degrees
                label.Rotation = (float)angle - 90; // -90 text orientation
            }
            double tmpx = line.Vertices[midPoint].X + (dx * 0.5);
            double tmpy = line.Vertices[midPoint].Y + (dy * 0.5);
            label.LabelPoint = viewportTransform.WorldToScreen(new Geometries.Point(tmpx, tmpy));
        }


    }
}
