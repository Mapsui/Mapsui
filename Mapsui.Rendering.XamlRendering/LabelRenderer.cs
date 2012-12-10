using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System.Globalization;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WinPoint = System.Windows.Point;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using WinPoint = Windows.Foundation.Point;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    public static class LabelRenderer
    {
        public static Canvas RenderStackedLabelLayer(IViewport viewport, LabelLayer layer)
        {
            var canvas = new Canvas();
            canvas.Opacity = layer.Opacity;

            //todo: take into account the priority 
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution);
            var margin = viewport.Resolution * 50;

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                var clusters = new List<Cluster>();
                //todo: repeat until there are no more merges
                ClusterFeatures(clusters, features, margin, layerStyle, viewport.Resolution);

                foreach (var cluster in clusters)
                {
                    Offset stackOffset = null;
                    
                    foreach (var feature in cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

                        if (stackOffset == null) //first time
                        {
                            stackOffset = new Offset();
                            if (cluster.Features.Count > 1)
                                canvas.Children.Add(RenderBox(cluster.Box, viewport));
                        }
                        else stackOffset.Y += 18; //todo: get size from text, (or just pass stack nr)
                                                
                        if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
                        var labelStyle = style as LabelStyle;
                        string labelText = layer.GetLabel(feature);
                        var position = new Mapsui.Geometries.Point(cluster.Box.GetCentroid().X, cluster.Box.Bottom);
                        canvas.Children.Add(RenderLabel(position, stackOffset, labelStyle, viewport, labelText));
                    }
                }
            }

            return canvas;
        }

        private static UIElement RenderBox(BoundingBox box, IViewport viewport)
        {
            const int margin = 32;
            const int halfMargin = margin / 2;

            var p1 = viewport.WorldToScreen(box.Min);
            var p2 = viewport.WorldToScreen(box.Max);

            var rectangle = new Rectangle();
            rectangle.Width = p2.X - p1.X + margin;
            rectangle.Height = p1.Y - p2.Y + margin;
            Canvas.SetLeft(rectangle, p1.X - halfMargin);
            Canvas.SetTop(rectangle, p2.Y - halfMargin);

            rectangle.Stroke = new SolidColorBrush(Colors.White);
            rectangle.StrokeThickness = 2;

            return rectangle;
        }

        public static Canvas RenderLabelLayer(IViewport viewport, LabelLayer layer)
        {
            var canvas = new Canvas();
            canvas.Opacity = layer.Opacity;

            //todo: take into account the priority 
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
            var stackOffset = new Offset();

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;
                    if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
                    var labelStyle = style as LabelStyle;
                    string labelText = layer.GetLabel(feature);
                    canvas.Children.Add(RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), 
                        stackOffset, labelStyle, viewport, labelText));
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

        public static UIElement RenderLabel(Mapsui.Geometries.Point point, Offset stackOffset, LabelStyle style, IViewport viewport)
        {
            return RenderLabel(point, stackOffset, style, viewport, style.Text);
        }

        public static UIElement RenderLabel(Mapsui.Geometries.Point point, Offset stackOffset, LabelStyle style, IViewport viewport, string text)
        {
            Mapsui.Geometries.Point p = viewport.WorldToScreen(point);
            var windowsPoint = new WinPoint(p.X, p.Y);

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

            var textWidth = textblock.ActualWidth;
            var textHeight = textblock.ActualHeight;
#if !SILVERLIGHT && !NETFX_CORE
            // in WPF the width and height is not calculated at this point. So we use FormattedText
            getTextWidthAndHeight(ref textWidth, ref textHeight, style, text);
#endif
            border.SetValue(Canvas.LeftProperty, windowsPoint.X + style.Offset.X + stackOffset.X - (textWidth + 2 * witdhMargin) * (short)style.HorizontalAlignment * 0.5f);
            border.SetValue(Canvas.TopProperty, windowsPoint.Y + style.Offset.Y + stackOffset.Y - (textHeight + 2 * heightMargin) * (short)style.VerticalAlignment * 0.5f);
                
            return border;
        }

#if !SILVERLIGHT && !NETFX_CORE
        private static void getTextWidthAndHeight(ref double width, ref double height, LabelStyle style, string text)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(style.Font.FontFamily),
                style.Font.Size,
                new SolidColorBrush(style.ForeColor.Convert()));

            width = formattedText.Width;
            height = formattedText.Height;
        }

#endif

        private class Cluster
        {
            public BoundingBox Box { get; set; }
            public IList<IFeature> Features { get; set; }
        }
    }
}
