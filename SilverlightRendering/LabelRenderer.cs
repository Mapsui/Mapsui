using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SharpMap.Styles;
using SharpMap.Styles.Thematics;

namespace SilverlightRendering
{
    public class LabelRenderer
    {
        public static Canvas RenderStackedLabelLayer(IView view, LabelLayer layer)
        {
            var canvas = new Canvas();
            canvas.Opacity = layer.Opacity;

            //todo: take into account the priority 
            var features = layer.GetFeaturesInView(view.Extent, view.Resolution);
            var margin = view.Resolution * 50;

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                var clusters = new List<Cluster>();
                //todo: repeat until there are no more merges
                ClusterFeatures(clusters, features, margin, layerStyle, view.Resolution);

                foreach (var cluster in clusters)
                {
                    Offset stackOffset = null;
                    
                    foreach (var feature in cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > view.Resolution) || (style.MaxVisible < view.Resolution)) continue;

                        if (stackOffset == null) //first time
                        {
                            stackOffset = new Offset();
                            if (cluster.Features.Count > 1)
                                canvas.Children.Add(RenderBox(cluster.Box, view));
                        }
                        else stackOffset.Y += 18; //todo: get size from text, (or just pass stack nr)
                                                
                        if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
                        var labelStyle = style as LabelStyle;
                        string labelText = layer.GetLabel(feature);
                        var position = new SharpMap.Geometries.Point(cluster.Box.GetCentroid().X, cluster.Box.Bottom);
                        canvas.Children.Add(RenderLabel(position, stackOffset, labelStyle, view, labelText));
                    }
                }
            }

            return canvas;
        }

        private static UIElement RenderBox(BoundingBox box, IView view)
        {
            const int margin = 32;
            const int halfMargin = margin / 2;

            var p1 = view.WorldToView(box.Min);
            var p2 = view.WorldToView(box.Max);

            var rectangle = new Rectangle();
            rectangle.Width = p2.X - p1.X + margin;
            rectangle.Height = p1.Y - p2.Y + margin;
            Canvas.SetLeft(rectangle, p1.X - halfMargin);
            Canvas.SetTop(rectangle, p2.Y - halfMargin);

            rectangle.Stroke = new SolidColorBrush(Colors.White);
            rectangle.StrokeThickness = 2;

            return rectangle;
        }

        public static Canvas RenderLabelLayer(IView view, LabelLayer layer)
        {
            var canvas = new Canvas();
            canvas.Opacity = layer.Opacity;

            //todo: take into account the priority 
            var features = layer.GetFeaturesInView(view.Extent, view.Resolution);
            var stackOffset = new Offset();

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style == null) || (style.Enabled == false) || (style.MinVisible > view.Resolution) || (style.MaxVisible < view.Resolution)) continue;
                    if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
                    var labelStyle = style as LabelStyle;
                    string labelText = layer.GetLabel(feature);
                    canvas.Children.Add(RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), 
                        stackOffset, labelStyle, view, labelText));
                }
            }

            return canvas;
        }

        private static void ClusterFeatures(
            IList<Cluster> clusters, 
            IEnumerable<IFeature> features, 
            double minDistance,
            IStyle layerStyle, 
            double resolution)
        {
            var style = layerStyle;
            //this method should repeated several times until there are no more merges
            foreach (var feature in features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
            {
                if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                if ((style == null) || (style.Enabled == false) || (style.MinVisible > resolution) || (style.MaxVisible < resolution)) continue;
                
                var found = false;
                foreach (var cluster in clusters)
                {
                    //todo: use actual overlap of labels not just proximity of geometries.
                    if (cluster.Box.Grow(minDistance).Contains(feature.Geometry.GetBoundingBox().GetCentroid()))
                    {
                        cluster.Features.Add(feature);
                        cluster.Box = cluster.Box.Join(feature.Geometry.GetBoundingBox());
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var cluster = new Cluster();
                    cluster.Box = feature.Geometry.GetBoundingBox().Clone();
                    cluster.Features = new List<IFeature>();
                    cluster.Features.Add(feature);
                    clusters.Add(cluster);
                }
            }
        }

        public static UIElement RenderLabel(SharpMap.Geometries.Point point, Offset stackOffset, LabelStyle style, IView view, string text)
        {
            SharpMap.Geometries.Point p = view.WorldToView(point);
            var windowsPoint = new System.Windows.Point(p.X, p.Y);

            var border = new Border();
            var textblock = new TextBlock();

            //Text
            textblock.Text = text;

            //Colors
            textblock.Foreground = new SolidColorBrush(style.ForeColor.Convert());
            border.Background = new SolidColorBrush(style.BackColor.Color.Convert());

            //Font
            textblock.FontFamily = new FontFamily(style.Font.FontFamily);
            textblock.FontSize = style.Font.Size;

            //set some defaults which should be configurable someday
            const double witdhMargin = 3.0;
            const double heightMargin = 0.0;
            textblock.Margin = new Thickness(witdhMargin, heightMargin, witdhMargin, heightMargin);
            border.CornerRadius = new CornerRadius(4);
            border.Child = textblock;

            //Offset
            border.SetValue(Canvas.LeftProperty, windowsPoint.X + style.Offset.X + stackOffset.X - (textblock.ActualWidth + 2 * witdhMargin) * (short)style.HorizontalAlignment * 0.5f);
            border.SetValue(Canvas.TopProperty, windowsPoint.Y + style.Offset.Y + stackOffset.Y - (textblock.ActualHeight + 2 * heightMargin) * (short)style.VerticalAlignment * 0.5f);

            //!!!grid.Effect = GeometryRenderer.CreateDropShadow(-90);

            return border;
        }

        private class Cluster
        {
            public BoundingBox Box { get; set; }
            public IList<IFeature> Features { get; set; }
        }
    }
}
