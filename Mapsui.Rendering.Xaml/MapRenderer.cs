using System.IO;
using Mapsui.Providers;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Logging;
using Color = Mapsui.Styles.Color;
using Polygon = Mapsui.Geometries.Polygon;
using System.Threading;
using Mapsui.Utilities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using XamlMedia = System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public class MapRenderer : IRenderer
    {
        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public void Render(object target, IViewport viewport, IEnumerable<ILayer> layers, Color background)
        {
            Render((Canvas) target, viewport, layers, background, false);
        }

        private static void Render(Canvas target, IViewport viewport, IEnumerable<ILayer> layers,
            Color background, bool rasterizing)
        {

            target.BeginInit();

            target.Background = background == null ? null : new XamlMedia.SolidColorBrush {Color = background.ToXaml()};

            target.Visibility = Visibility.Collapsed;

            foreach (var child in target.Children)
            {
                (child as Canvas)?.Children.Clear();
            }

            target.Children.Clear();

            layers = layers.ToList();

            foreach (var layer in layers)
            {
                if (!layer.Enabled) continue;
                if (layer.MinVisible > viewport.Resolution) continue;
                if (layer.MaxVisible < viewport.Resolution) continue;

                RenderLayer(target, viewport, layer, rasterizing);
            }
            target.Arrange(new Rect(0, 0, viewport.Width, viewport.Height));
            target.Visibility = Visibility.Visible;
            
            if (DeveloperTools.DeveloperMode)
            {
                DrawDebugInfo(target, layers);
            }

            target.EndInit();
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers, Color background = null)
        {
            MemoryStream bitmapStream = null;
            RunMethodOnStaThread(() => bitmapStream = RenderToBitmapStreamStatic(viewport, layers, background));
            return bitmapStream;
        }
        
        private static MemoryStream RenderToBitmapStreamStatic(IViewport viewport, IEnumerable<ILayer> layers, Color background)
        {
            var canvas = new Canvas();
            Render(canvas, viewport, layers, background, true);
            var bitmapStream = BitmapRendering.BitmapConverter.ToBitmapStream(canvas, (int)viewport.Width, (int)viewport.Height);
            canvas.Children.Clear();
            canvas.Dispatcher.InvokeShutdown();
            return bitmapStream;
        }

        private static void RunMethodOnStaThread(ThreadStart operation)
        {
            var thread = new Thread(operation);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            thread.Join();
        }

        public static void RenderLayer(Canvas target, IViewport viewport, ILayer layer, bool rasterizing = false)
        {
            if (layer.Enabled == false) return;

            target.Children.Add(RenderVectorLayer(viewport, layer, rasterizing));
        }

        private static Canvas RenderVectorLayer(IViewport viewport, ILayer layer, bool rasterizing = false)
        {
            // todo:
            // find solution for try catch. Sometimes this method will throw an exception
            // when clearing and adding features to a layer while rendering
            var canvas = new Canvas
            {
                Opacity = layer.Opacity,
                IsHitTestVisible = false
            };

            try
            {
                var features = layer.GetFeaturesInView(viewport.Extent, viewport.RenderResolution).ToList();
                var layerStyles = BaseLayer.GetLayerStyles(layer);
                var brushCache = new BrushCache();

                foreach (var layerStyle in layerStyles)
                {
                    var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                    foreach (var feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) ||
                            (style.MaxVisible < viewport.Resolution)) continue;

                        RenderFeature(viewport, canvas, feature, style, rasterizing, brushCache);
                    }
                }

                foreach (var feature in features)
                {
                    var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
                    foreach (var style in styles)
                        if ((feature.Styles != null) && style.Enabled)
                            RenderFeature(viewport, canvas, feature, style, rasterizing, brushCache);
                }

                return canvas;
            }
            catch (Exception ex)
            {
                Logger.LogDelegate(LogLevel.Error, "Unexpected error in renderer", ex);
                return canvas;
                // If exception happens inside RenderFeature function after 
                // at -least one child has been added to the canvas,
                // returning new canvas will leave the previously created (but 
                // not yet added to parent canvas) canvas abandoned, that will 
                // cause the exception when resuing RenderedGeometry object, because 
                // at -least one RenderedGeometry was attached to that abandoned canvas.
                // returning the same canvas will solve this error, as it will 
                // be clear this canvas childs on next render call.
                // return new Canvas { IsHitTestVisible = false };
            }
        }

        private static void RenderFeature(IViewport viewport, Canvas canvas, IFeature feature, IStyle style,
            bool rasterizing, BrushCache brushCache = null)
        {
            if (style is LabelStyle)
            {
                var labelStyle = (LabelStyle) style;
                canvas.Children.Add(SingleLabelRenderer.RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(),
                    labelStyle, viewport, labelStyle.GetLabelText(feature)));
            }
            else
            {
                var renderedGeometry = feature.RenderedGeometry.ContainsKey(style)
                    ? feature.RenderedGeometry[style] as Shape
                    : null;
                if (renderedGeometry == null)
                {
                    renderedGeometry = RenderGeometry(viewport, style, feature, brushCache);
                    if (!rasterizing) feature.RenderedGeometry[style] = renderedGeometry;
                }
                else
                {
                    PositionGeometry(renderedGeometry, viewport, style, feature);
                }

                if (!canvas.Children.Contains(renderedGeometry))
                    // Adding twice can happen when a single feature has two identical styles
                    canvas.Children.Add(renderedGeometry);
            }
        }

        private static Shape RenderGeometry(IViewport viewport, IStyle style, IFeature feature,
            BrushCache brushCache = null)
        {
            if (feature.Geometry is Geometries.Point)
                return PointRenderer.RenderPoint(feature.Geometry as Geometries.Point, style, viewport, brushCache);
            if (feature.Geometry is MultiPoint)
                return GeometryRenderer.RenderMultiPoint(feature.Geometry as MultiPoint, style, viewport);
            if (feature.Geometry is LineString)
                return GeometryRenderer.RenderLineString(feature.Geometry as LineString, style, viewport);
            if (feature.Geometry is MultiLineString)
                return GeometryRenderer.RenderMultiLineString(feature.Geometry as MultiLineString, style, viewport);
            if (feature.Geometry is Polygon)
                return GeometryRenderer.RenderPolygon(feature.Geometry as Polygon, style, viewport, brushCache);
            if (feature.Geometry is MultiPolygon)
                return GeometryRenderer.RenderMultiPolygon(feature.Geometry as MultiPolygon, style, viewport);
            if (feature.Geometry is IRaster)
                return GeometryRenderer.RenderRaster(feature.Geometry as IRaster, style, viewport);
            return null;
        }

        private static void PositionGeometry(Shape renderedGeometry, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Geometries.Point)
                PointRenderer.PositionPoint(renderedGeometry, feature.Geometry as Geometries.Point, style, viewport);
            else if (feature.Geometry is MultiPoint)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            else if (feature.Geometry is LineString)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            else if (feature.Geometry is MultiLineString)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            else if (feature.Geometry is Polygon)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            else if (feature.Geometry is MultiPolygon)
                GeometryRenderer.PositionGeometry(renderedGeometry, viewport);
            else if (feature.Geometry is IRaster)
                GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), viewport);
        }

        private static void DrawDebugInfo(Canvas canvas, IEnumerable<ILayer> layers)
        {
            var lineCounter = 1;
            const float tabWidth = 40f;
            const float lineHeight = 40f;

            foreach (var layer in layers)
            {
                var textBox = AddTextBox(layer.ToString(), tabWidth, lineHeight * (lineCounter++));
                canvas.Children.Add(textBox);

                if (layer is ITileLayer)
                {
                    var text = "Tiles in memory: " + (layer as ITileLayer).MemoryCache.TileCount.ToString(CultureInfo.InvariantCulture);
                    canvas.Children.Add(AddTextBox(text, tabWidth, lineHeight * lineCounter++));
                }
            }
        }

        private static TextBox AddTextBox(string text, float x, float y)
        {
            var textBox = new TextBox { Text = text };
            Canvas.SetLeft(textBox, x);
            Canvas.SetTop(textBox, y);
            return textBox;
        }
    }
}