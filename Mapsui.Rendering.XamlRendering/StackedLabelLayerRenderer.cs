using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    public static class StackedLabelLayerRenderer
    {
        public static Canvas Render(IViewport viewport, LabelLayer layer)
        {
            // todo: Move stack functionality to Mapsui core.
            // step 1) Split RenderStackedLabelLayer into a method
            // GetFeaturesInViewStacked en a RenderStackedLabel 
            // which can later be replace by normal label rendering.
            // The method GetFeaturesInViewStacked 
            // returns a style with an offset determined by the stackoffset
            // and a position determined by CenterX en Cluster.Box.Bottom.
            // step 2) Move GetFeaturesInViewStacked to a GetFeaturesInView
            // method of a new StackedLabelLayed.

            if (!(layer.Style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
            var layerStyle = layer.Style as LabelStyle;

            var canvas = new Canvas { Opacity = layer.Opacity };

            // todo: take into account the priority 
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToArray();
            var margin = viewport.Resolution * 50;

            const int symbolSize = 32; // todo: determine margin by symbol size
            const int boxMargin = symbolSize / 2;

            var clusters = new List<Cluster>();
            //todo: repeat until there are no more merges
            ClusterFeatures(clusters, features, margin, layer.Style, viewport.Resolution);
            const int textHeight = 18;
            foreach (var cluster in clusters)
            {
                var stackOffsetY = double.NaN;

                var orderedFeatures = cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y);

                if (cluster.Features.Count > 1) canvas.Children.Add(RenderBox(cluster.Box, viewport));

                foreach (var feature in orderedFeatures)
                {
                    if (double.IsNaN(stackOffsetY)) // first time
                        stackOffsetY = textHeight * 0.5 + boxMargin;
                    else
                        stackOffsetY += textHeight; //todo: get size from text (or just pass stack nr)

                    var labelStyle = new LabelStyle(layerStyle as LabelStyle)
                    {
                        Text = layer.GetLabelText(feature)
                    };
                    labelStyle.Offset.Y += stackOffsetY;

                    var position = new Geometries.Point(cluster.Box.GetCentroid().X, cluster.Box.Bottom);

                    canvas.Children.Add(SingleLabelRenderer.RenderLabel(position, labelStyle, viewport));
                }
            }
            return canvas;
        }

        private static UIElement RenderBox(BoundingBox box, IViewport viewport)
        {
            const int symbolSize = 32; // todo: determine margin by symbol size
            const int boxMargin = symbolSize / 2;

            var p1 = viewport.WorldToScreen(box.Min);
            var p2 = viewport.WorldToScreen(box.Max);

            var rectangle = new Rectangle
            {
                Width = p2.X - p1.X + symbolSize,
                Height = p1.Y - p2.Y + symbolSize
            };

            Canvas.SetLeft(rectangle, p1.X - boxMargin);
            Canvas.SetTop(rectangle, p2.Y - boxMargin);

            rectangle.Stroke = new SolidColorBrush(Colors.White);
            rectangle.StrokeThickness = 2;

            return rectangle;
        }

        private static void ClusterFeatures(
           ICollection<Cluster> clusters,
           IEnumerable<IFeature> features,
           double minDistance,
           IStyle layerStyle,
           double resolution)
        {
            var style = layerStyle;

            // This method should repeated several times until there are no more merges
            foreach (var feature in features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
            {
                if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                if ((style == null) ||
                    (style.Enabled == false) ||
                    (style.MinVisible > resolution) ||
                    (style.MaxVisible < resolution)) continue;

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

                if (found) continue;
                clusters.Add(new Cluster
                {
                    Box = feature.Geometry.GetBoundingBox().Clone(),
                    Features = new List<IFeature> { feature }
                });
            }
        }

        private class Cluster
        {
            public BoundingBox Box { get; set; }
            public IList<IFeature> Features { get; set; }
        }
    }
}
