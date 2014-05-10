using System.Globalization;
using System.IO;
using System.Threading;
using Mapsui.Providers;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;
#if !NETFX_CORE
using System.Windows;
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
#endif

namespace Mapsui.Rendering.Xaml
{
    public class MapRenderer : IRenderer
    {
        private readonly Canvas _target;
#if !NETFX_CORE
        private static int _mainThreadId;
#endif

        static MapRenderer()
        {
#if !NETFX_CORE
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public MapRenderer()
        {
            _target = new Canvas { IsHitTestVisible = false };
        }

        public MapRenderer(Canvas target)
        {
            _target = target;
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(_target, viewport, layers, false);
        }

        public static void Render(Canvas target, IViewport viewport, IEnumerable<ILayer> layers, bool rasterizing)
        {
#if !SILVERLIGHT &&  !NETFX_CORE
            target.BeginInit();
#endif
            //var layers = inLayers.ToList();
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
                    RenderLayer(target, viewport, layer, rasterizing);
                }
            }
            target.Arrange(new Rect(0, 0, viewport.Width, viewport.Height));
            target.Visibility = Visibility.Visible;

            //DrawDebugInfo(target, layers);

#if !SILVERLIGHT &&  !NETFX_CORE
            target.EndInit();
#endif
        }

#if !SILVERLIGHT &&  !NETFX_CORE && !WINDOWS_PHONE
        private static void DrawDebugInfo(Canvas canvas, IEnumerable<ILayer> layers)
        {
            var lineCounter = 1;
            const float tabWidth = 40f;
            const float lineHeight = 40f;

            foreach (var layer in layers)
            {
                var textBox = AddTextBox(layer.ToString(), tabWidth, lineHeight*(lineCounter++));
                canvas.Children.Add(textBox);

                if (layer is ITileLayer)
                {
                    var text = "Tiles in memory: " + (layer as ITileLayer).MemoryCache.TileCount.ToString(CultureInfo.InvariantCulture);
                    canvas.Children.Add(AddTextBox(text, tabWidth, lineHeight*(lineCounter++)));
                }
            }
        }

        private static TextBox AddTextBox(string text, float x, float y)
        {
            var textBox = new TextBox {Text = text};
            Canvas.SetLeft(textBox, x);
            Canvas.SetTop(textBox, y);
            return textBox;
        }
#endif
        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
#if WINDOWS_PHONE || NETFX_CORE
            throw new NotImplementedException();
#elif SILVERLIGHT
            MemoryStream bitmapStream = null;
            var waitHandle = new AutoResetEvent(false);
            RunOnUIThread(() =>
            {
                bitmapStream = RenderToBitmapStreamStatic(viewport, layers);
                waitHandle.Set();
            });
            waitHandle.WaitOne(60000);
            return bitmapStream;
#else
            MemoryStream bitmapStream = null;
            RunMethodOnStaThread(() => bitmapStream = RenderToBitmapStreamStatic(viewport, layers));
            return bitmapStream;
#endif
        }

#if !WINDOWS_PHONE && !NETFX_CORE
        private static MemoryStream RenderToBitmapStreamStatic(IViewport viewport, IEnumerable<ILayer> layers)
        {
            var canvas = new Canvas();
            Render(canvas, viewport, layers, true);
            var bitmapStream = BitmapRendering.BitmapConverter.ToBitmapStream(canvas, (int)viewport.Width, (int)viewport.Height);
            canvas.Children.Clear();
            return bitmapStream;
        }
#endif

#if SILVERLIGHT && !WINDOWS_PHONE
        private void RunOnUIThread(Action method)
        {
            Deployment.Current.Dispatcher.BeginInvoke(method);
        }
#endif

#if !SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE
        private static void RunMethodOnStaThread(ThreadStart operation)
        {
            var thread = new Thread(operation);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            thread.Join();
        }
#endif

        public static void RenderLayer(Canvas target, IViewport viewport, ILayer layer, bool rasterizing = false)
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
                target.Children.Add(RenderVectorLayer(viewport, layer, rasterizing));
            }
        }

        private static Canvas RenderVectorLayer(IViewport viewport, ILayer layer, bool rasterizing = false)
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

                var features = layer.GetFeaturesInView(viewport.Extent, viewport.RenderResolution).ToList();
                var layerStyles = BaseLayer.GetLayerStyles(layer);

                foreach (var layerStyle in layerStyles)
                {
                    var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                    foreach (var feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

                        RenderFeature(viewport, canvas, feature, style, rasterizing);
                    }
                }

                foreach (var feature in features)
                {
                    var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
                    foreach (var style in styles)
                    {
                        if (feature.Styles != null && style.Enabled)
                        {
                            RenderFeature(viewport, canvas, feature, style, rasterizing);
                        }
                    }
                }

                return canvas;
            }
            catch (Exception)
            {
                return new Canvas { IsHitTestVisible = false };
            }
        }

        private static void RenderFeature(IViewport viewport, Canvas canvas, IFeature feature, IStyle style, bool rasterizing)
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
                    if (!rasterizing) feature.RenderedGeometry[style] = renderedGeometry;
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
    }
}

