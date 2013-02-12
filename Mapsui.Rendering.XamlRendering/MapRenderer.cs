using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AnimateEventHandler = System.EventHandler;
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using AnimateEventHandler = System.EventHandler<object>;
#endif
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Rendering.XamlRendering
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

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
#if !SILVERLIGHT &&  !NETFX_CORE
            target.BeginInit();
#endif
            target.Visibility = Visibility.Collapsed;
            foreach (var child in target.Children)
            {
                if (child is Canvas)
                {
                    (child as Canvas).Children.Clear();
                }
            }
            target.Children.Clear();

            foreach (var layer in layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= viewport.Resolution &&
                    layer.MaxVisible >= viewport.Resolution)
                {
                    RenderLayer(target, viewport, layer);
                }
            }
            target.Arrange(new Rect(0, 0, viewport.Width, viewport.Height));
            target.Visibility = Visibility.Visible;
#if !SILVERLIGHT &&  !NETFX_CORE
            target.EndInit();
#endif
        }

        private static void RenderLayer(Canvas target, IViewport viewport, ILayer layer)
        {
            if (layer.Enabled == false) return;

            if (layer is LabelLayer)
            {
                var labelLayer = layer as LabelLayer;
                target.Children.Add(labelLayer.UseLabelStacking
                    ? LabelRenderer.RenderStackedLabelLayer(viewport, labelLayer)
                    : LabelRenderer.RenderLabelLayer(viewport, labelLayer));
            }
            else
            {
                target.Children.Add(RenderVectorLayer(viewport, layer));
            }
        }

        private static Canvas RenderVectorLayer(IViewport viewport, ILayer layer)
        {
            // todo:
            // find solution for try catch. Sometimes this method will throw an exception
            // when clearing and adding features to a layer while rendering
            try
            {
                var canvas = new Canvas {Opacity = layer.Opacity};
                var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();

                foreach (var layerStyle in layer.Styles)
                {
                    var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                    foreach (var feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

                        RenderFeature(canvas, viewport, style, feature);
                    }
                }

                foreach (var feature in features)
                {
                    var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
                    foreach (var style in styles)
                    {
                        if (feature.Styles != null && style.Enabled)
                        {
                            RenderFeature(canvas, viewport, style, feature);
                        }
                    }
                }

                return canvas;
            }
            catch (Exception)
            {
                return new Canvas();
            }
        }

        private static void RenderFeature(Canvas canvas, IViewport viewport, IStyle style, Providers.IFeature feature)
        {
            if (style is LabelStyle)
            {
                canvas.Children.Add(LabelRenderer.RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), new Offset(), style as LabelStyle, viewport));
            }
            else
            {
                var renderedGeometry = feature.RenderedGeometry.ContainsKey(style) ? feature.RenderedGeometry[style] as UIElement : null;
                if (renderedGeometry == null)
                {
                    renderedGeometry = RenderGeometry(viewport, style, feature);
                    if (feature.Geometry is Geometries.Point || feature.Geometry is IRaster) // positioning only supported for point and raster
                        feature.RenderedGeometry[style] = renderedGeometry;
                }
                else
                {
                    PositionGeometry(renderedGeometry, viewport, style, feature);
                }
                canvas.Children.Add(renderedGeometry);
            }
        }

        private static UIElement RenderGeometry(IViewport viewport, IStyle style, Providers.IFeature feature)
        {
            if (feature.Geometry is Geometries.Point)
                return GeometryRenderer.RenderPoint(feature.Geometry as Geometries.Point, style, viewport);
            if (feature.Geometry is MultiPoint)
                return GeometryRenderer.RenderMultiPoint(feature.Geometry as MultiPoint, style, viewport);
            if (feature.Geometry is LineString)
                return GeometryRenderer.RenderLineString(feature.Geometry as LineString, style, viewport);
            if (feature.Geometry is MultiLineString)
                return GeometryRenderer.RenderMultiLineString(feature.Geometry as MultiLineString, style, viewport);
            if (feature.Geometry is Polygon)
                return GeometryRenderer.RenderPolygon(feature.Geometry as Polygon, style, viewport);
            if (feature.Geometry is MultiPolygon)
                return GeometryRenderer.RenderMultiPolygon(feature.Geometry as MultiPolygon, style, viewport);
            if (feature.Geometry is IRaster)
                return GeometryRenderer.RenderRaster(feature.Geometry as IRaster, style, viewport);
            return null;
        }

        private static void PositionGeometry(UIElement renderedGeometry, IViewport viewport, IStyle style, Providers.IFeature feature)
        {
            if (feature.Geometry is Geometries.Point)
                GeometryRenderer.PositionPoint(renderedGeometry, feature.Geometry as Geometries.Point, style, viewport);
            if (feature.Geometry is MultiPoint)
                return;
            if (feature.Geometry is LineString)
                return;
            if (feature.Geometry is MultiLineString)
                return;
            if (feature.Geometry is Polygon)
                return;
            if (feature.Geometry is MultiPolygon)
                return;
            if (feature.Geometry is IRaster)
                GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), viewport);
        }

        public static void Animate(DependencyObject target, string property, double from, double to, int duration, AnimateEventHandler completed)
        {
            var animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new TimeSpan(0, 0, 0, 0, duration);
            Storyboard.SetTarget(animation, target);
#if !NETFX_CORE
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
#else
            Storyboard.SetTargetProperty(animation, property);
#endif

            var storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            storyBoard.Completed += completed;
            storyBoard.Begin();
        }

#if !NETFX_CORE

        public MemoryStream ToBitmapStream(double width, double height)
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
#endif
    }
}
