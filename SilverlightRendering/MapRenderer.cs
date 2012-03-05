using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles.Thematics;

namespace SilverlightRendering
{
    public class MapRenderer : IRenderer
    {
        private readonly Canvas target;

        public MapRenderer()
        {
            target = new Canvas();
        }

        public MapRenderer(Canvas target)
        {
            this.target = target;
        }

        public void Render(IView view, LayerCollection layers)
        {
            foreach (var child in target.Children)
            {
                if (child is Canvas) (child as Canvas).Children.Clear();
            }
            target.Children.Clear();
                        
            foreach (var layer in layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= view.Resolution &&
                    layer.MaxVisible >= view.Resolution)
                {
                    RenderLayer(target, view, layer);
                }
            }
            target.Arrange(new Rect(0, 0, view.Width, view.Height));
        }

        private static void RenderLayer(Canvas target, IView view, ILayer layer)
        {
            if (layer.Enabled == false) return;

            if (layer is LabelLayer)
            {
                var labelLayer = layer as LabelLayer;
                if (labelLayer.UseLabelStacking)
                {
                    target.Children.Add(LabelRenderer.RenderStackedLabelLayer(view, labelLayer));
                }
                else
                {
                    target.Children.Add(LabelRenderer.RenderLabelLayer(view, labelLayer));
                }
            }
            else
            {
                target.Children.Add(RenderVectorLayer(view, layer));
            }
        }

        private static Canvas RenderVectorLayer(IView view, ILayer layer)
        {
            var canvas = new Canvas();
            canvas.Opacity = layer.Opacity;

            var features = layer.GetFeaturesInView(view.Extent, view.Resolution).ToList();

            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    if ((style == null) || (style.Enabled == false) || (style.MinVisible > view.Resolution) || (style.MaxVisible < view.Resolution)) continue;

                    RenderGeometry(canvas, view, style, feature);
                }
            }

            foreach (var feature in features)
            {
                if (feature.Style != null)
                {
                    RenderGeometry(canvas, view, feature.Style, feature);
                }
            }

            return canvas;
        }

        private static void RenderGeometry(Canvas canvas, IView view, SharpMap.Styles.IStyle style, SharpMap.Providers.IFeature feature)
        {
            if (feature.Geometry is SharpMap.Geometries.Point)
                canvas.Children.Add(GeometryRenderer.RenderPoint(feature.Geometry as SharpMap.Geometries.Point, style, view));
            else if (feature.Geometry is MultiPoint)
                canvas.Children.Add(GeometryRenderer.RenderMultiPoint(feature.Geometry as MultiPoint, style, view));
            else if (feature.Geometry is LineString)
                canvas.Children.Add(GeometryRenderer.RenderLineString(feature.Geometry as LineString, style, view));
            else if (feature.Geometry is MultiLineString)
                canvas.Children.Add(GeometryRenderer.RenderMultiLineString(feature.Geometry as MultiLineString, style, view));
            else if (feature.Geometry is Polygon)
                canvas.Children.Add(GeometryRenderer.RenderPolygon(feature.Geometry as Polygon, style, view));
            else if (feature.Geometry is MultiPolygon)
                canvas.Children.Add(GeometryRenderer.RenderMultiPolygon(feature.Geometry as MultiPolygon, style, view));
            else if (feature.Geometry is IRaster)
            {
                var renderedGeometry = feature.RenderedGeometry as UIElement;
                if (renderedGeometry == null) // create
                {
                    renderedGeometry = GeometryRenderer.RenderRaster(feature.Geometry as IRaster, style, view);
                    Animate(renderedGeometry, "Opacity", 0, 1, 600, (s, e) => { });
                    feature.RenderedGeometry = renderedGeometry;
                }
                else // position
                {
                    GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), view);
                }
                canvas.Children.Add(renderedGeometry);
            }
        }

        public static void Animate(DependencyObject target, string property, double from, double to, int duration, EventHandler completed)
        {
            var animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new TimeSpan(0, 0, 0, 0, duration);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            var storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            storyBoard.Completed += completed;
            storyBoard.Begin();
        }

        public Stream ToBitmapStream(double width, double height)
        {
            target.Arrange(new Rect(0, 0, width, height));
#if !SILVERLIGHT
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new PixelFormat());
            renderTargetBitmap.Render(target);
            var bitmap = new PngBitmapEncoder();
            bitmap.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            var bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream);
#else
            var writeableBitmap = new WriteableBitmap((int)width, (int)height);
            writeableBitmap.Render(target, null);
            var bitmapStream = Utilities.ConverToBitmapStream(writeableBitmap);
#endif
            return bitmapStream;
        }
    }
}
