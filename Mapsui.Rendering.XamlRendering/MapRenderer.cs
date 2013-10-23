using Mapsui.Providers;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;
#if !NETFX_CORE
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AnimateEventHandler = System.EventHandler;
#if !SILVERLIGHT
using System.Windows.Media;
#endif
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using AnimateEventHandler = System.EventHandler<object>;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    public class MapRenderer : IRenderer
    {
        private readonly Canvas _target;

        public MapRenderer()
        {
            _target = new Canvas();
        }

        public MapRenderer(Canvas target)
        {
            _target = target;
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
#if !SILVERLIGHT &&  !NETFX_CORE
            _target.BeginInit();
#endif
            _target.Visibility = Visibility.Collapsed;
            foreach (var child in _target.Children)
            {
                if (child is Canvas)
                {
                    (child as Canvas).Children.Clear();
                }
            }
            _target.Children.Clear();

            foreach (var layer in layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= viewport.Resolution &&
                    layer.MaxVisible >= viewport.Resolution)
                {
                    RenderLayer(_target, viewport, layer);
                }
            }
            _target.Arrange(new Rect(0, 0, viewport.Width, viewport.Height));
            _target.Visibility = Visibility.Visible;
#if !SILVERLIGHT &&  !NETFX_CORE
            _target.EndInit();
#endif
        }

        internal static void RenderLayer(Canvas target, IViewport viewport, ILayer layer)
        {
            if (layer.Enabled == false) return;

            if (layer is LabelLayer)
            {
                var labelLayer = layer as LabelLayer;
                target.Children.Add(labelLayer.UseLabelStacking
                    ? StackedLabelLayerRenderer.Render(viewport, labelLayer)
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
                var canvas = new Canvas
                    {
                        Opacity = layer.Opacity, 
                        IsHitTestVisible = false
                    };

                var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
                var layerStyles = BaseLayer.GetLayerStyles(layer);
            
                foreach (var layerStyle in layerStyles)
                {
                    var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                    foreach (var feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

                        RenderFeature(viewport, canvas, feature, style);
                    }
                }

                foreach (var feature in features)
                {
                    var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
                    foreach (var style in styles)
                    {
                        if (feature.Styles != null && style.Enabled)
                        {
                            RenderFeature(viewport, canvas, feature, style);
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

        private static void RenderFeature(IViewport viewport, Canvas canvas, IFeature feature, IStyle style)
        {
            if (style is LabelStyle)
            {
                canvas.Children.Add(SingleLabelRenderer.RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), style as LabelStyle, viewport));
            }
            else
            {
                var renderedGeometry = feature.RenderedGeometry.ContainsKey(style) ? feature.RenderedGeometry[style] as UIElement : null;
                if (renderedGeometry == null)
                {
                    renderedGeometry = RenderGeometry(viewport, style, feature);
                    feature.RenderedGeometry[style] = renderedGeometry;
                }
                else
                {
                    PositionGeometry(renderedGeometry, viewport, style, feature);
                }

                if (!canvas.Children.Contains(renderedGeometry)) // Adding twice can happen when a single feature has two identical styles
                    canvas.Children.Add(renderedGeometry);
            }
        }

        private static UIElement RenderGeometry(IViewport viewport, IStyle style, IFeature feature)
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

        private static void PositionGeometry(UIElement renderedGeometry, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Geometries.Point)
                GeometryRenderer.PositionPoint(renderedGeometry, feature.Geometry as Geometries.Point, style, viewport);
            if (feature.Geometry is MultiPoint)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            if (feature.Geometry is LineString)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            if (feature.Geometry is MultiLineString)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            if (feature.Geometry is Polygon)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            if (feature.Geometry is MultiPolygon)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            if (feature.Geometry is IRaster)
                GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), viewport);
        }

        public static void Animate(DependencyObject target, string property, double from, double to, int duration, AnimateEventHandler completed)
        {
            return;

            var animation = new DoubleAnimation
                {
                    From = from, 
                    To = to, 
                    Duration = new TimeSpan(0, 0, 0, 0, duration)
                };

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
            return ToBitmapStream(_target, width, height);
        }

#if SILVERLIGHT

        public static MemoryStream ToBitmapStream(UIElement uiElement, double width, double height)
        {
            uiElement.Arrange(new Rect(0, 0, width, height));

            var writeableBitmap = new WriteableBitmap((int)width, (int)height);
            writeableBitmap.Render(uiElement, null);
            var bitmapStream = Utilities.ConverToBitmapStream(writeableBitmap);
            return bitmapStream;
        }
#else
        public static MemoryStream ToBitmapStream(UIElement uiElement, double width, double height)
        {
            uiElement.Arrange(new Rect(0, 0, width, height));
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new PixelFormat());
            renderTargetBitmap.Render(uiElement);
            var bitmap = new PngBitmapEncoder();
            bitmap.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            var bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream);
            return bitmapStream;
        }
#endif
#endif

    }
}

