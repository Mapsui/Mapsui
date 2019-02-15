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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Mapsui.Rendering.Xaml.XamlWidgets;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using XamlMedia = System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public class MapRenderer : IRenderer
    {
        private readonly SymbolCache _symbolCache = new SymbolCache();
        public ISymbolCache SymbolCache => _symbolCache;
        public IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } = new Dictionary<Type, IWidgetRenderer>();

        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public MapRenderer()
        {
            WidgetRenders[typeof(Hyperlink)] = new HyperlinkWidgetRenderer();
            WidgetRenders[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
            WidgetRenders[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
        }

        public void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
            IEnumerable<IWidget> widgets, Color background = null)
        {
            var allWidgets = layers.Select(l => l.Attribution).ToList().Where(w => w != null).Concat(widgets).ToList();

            RenderTypeSave((Canvas) target, viewport, layers, allWidgets, background);
        }
        private void RenderTypeSave(Canvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, 
            IEnumerable<IWidget> widgets, Color background = null)
        {
            if (!viewport.HasSize) return;

            Clear(canvas, background);
            Render(canvas, viewport, layers);
            Render(canvas, viewport, widgets);
        }

        private void Render(Canvas target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(target, viewport, layers,  _symbolCache, false);
        }

        private void Render(object target, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets)
        {
            WidgetRenderer.Render(target, viewport, widgets, WidgetRenders);
        }

        private static void Render(Canvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
            SymbolCache symbolCache, bool rasterizing)
        {
            canvas.BeginInit();
            canvas.Visibility = Visibility.Hidden;

            layers = layers.ToList();

            foreach (var layer in layers)
            {
                if (!layer.Enabled) continue;
                if (layer.MinVisible > viewport.Resolution) continue;
                if (layer.MaxVisible < viewport.Resolution) continue;

                RenderLayer(canvas, viewport, layer, symbolCache, rasterizing);
            }

            canvas.Visibility = Visibility.Visible;
            canvas.EndInit();
        }

        private static void Clear(Canvas canvas, Color background)
        {
            canvas.Background = ToBackground(background);

            foreach (var child in canvas.Children)
            {
                (child as Canvas)?.Children.Clear();
            }

            canvas.Children.Clear();
        }

        private static XamlMedia.SolidColorBrush ToBackground(Color background)
        {
            return background == null ? null : new XamlMedia.SolidColorBrush {Color = background.ToXaml()};
        }

        public MemoryStream RenderToBitmapStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color background = null)
        {
            MemoryStream bitmapStream = null;
            RunMethodOnStaThread(() => bitmapStream = RenderToBitmapStreamStatic(viewport, layers, _symbolCache));
            return bitmapStream;
        }
        
        private static MemoryStream RenderToBitmapStreamStatic(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, SymbolCache symbolCache)
        {
            var canvas = new Canvas();
            Render(canvas, viewport, layers, symbolCache, true);
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

        public static void RenderLayer(Canvas target, IReadOnlyViewport viewport, ILayer layer, SymbolCache symbolCache, bool rasterizing = false)
        {
            if (layer.Enabled == false) return;

            target.Children.Add(RenderLayerStatic(viewport, layer, symbolCache, rasterizing));
        }

        private static Canvas RenderLayerStatic(IReadOnlyViewport viewport, ILayer layer, SymbolCache symbolCache, bool rasterizing = false)
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
                var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
                var layerStyles = BaseLayer.GetLayerStyles(layer);

                // If rasterizing (which is usually on a background thread) create a new SymbolCache 
                // just for this rendering because cross thread access is not allowed in WPF.

                if (rasterizing) symbolCache = new SymbolCache();
                
                foreach (var layerStyle in layerStyles)
                {
                    var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                    foreach (var feature in features)
                    {
                        if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                        if (style == null || 
                            style.Enabled == false || 
                            style.MinVisible > viewport.Resolution ||
                            style.MaxVisible < viewport.Resolution) continue;

                        RenderFeature(viewport, canvas, feature, style, symbolCache, rasterizing);
                    }
                }

                foreach (var feature in features)
                {
                    var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
                    foreach (var style in styles)
                        if (feature.Styles != null && style.Enabled)
                            RenderFeature(viewport, canvas, feature, style, symbolCache, rasterizing);
                }

                return canvas;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unexpected error in xaml renderer", ex);
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

        private static void RenderFeature(IReadOnlyViewport viewport, Canvas canvas, IFeature feature, IStyle style, SymbolCache symbolCache, bool rasterizing)
        {
            if (style is LabelStyle)
            {
                var labelStyle = (LabelStyle) style;
                var labelText = labelStyle.GetLabelText(feature);
                if (string.IsNullOrEmpty(labelText)) return;
                canvas.Children.Add(LabelRenderer.RenderLabel(feature.Geometry.BoundingBox.Centroid,
                    labelStyle, viewport, labelText));
            }
            else
            {
                Shape renderedGeometry;
                if (feature.RenderedGeometry.TryGetValue(style, out var cachedObject))
                {
                    renderedGeometry = (Shape)cachedObject; // Has to be Shape
                    PositionGeometry(renderedGeometry, viewport, style, feature);
                }
                else
                {
                    renderedGeometry = RenderGeometry(viewport, style, feature, symbolCache);
                    if (!rasterizing) feature.RenderedGeometry[style] = renderedGeometry;
                }

                if (!canvas.Children.Contains(renderedGeometry))
                    // Adding twice can happen when a single feature has two identical styles
                    canvas.Children.Add(renderedGeometry);
            }
        }

        private static Shape RenderGeometry(IReadOnlyViewport viewport, IStyle style, IFeature feature,
            SymbolCache symbolCache)
        {
            if (feature.Geometry is Geometries.Point)
                return PointRenderer.RenderPoint(feature.Geometry as Geometries.Point, style, viewport, symbolCache);
            if (feature.Geometry is MultiPoint)
                return GeometryRenderer.RenderMultiPoint(feature.Geometry as MultiPoint, style, viewport, symbolCache);
            if (feature.Geometry is LineString)
                return LineStringRenderer.RenderLineString(feature.Geometry as LineString, style, viewport);
            if (feature.Geometry is MultiLineString)
                return MultiLineStringRenderer.Render(feature.Geometry as MultiLineString, style, viewport);
            if (feature.Geometry is Polygon)
                return PolygonRenderer.RenderPolygon(feature.Geometry as Polygon, style, viewport, symbolCache);
            if (feature.Geometry is MultiPolygon)
                return MultiPolygonRenderer.RenderMultiPolygon(feature.Geometry as MultiPolygon, style, viewport, symbolCache);
            if (feature.Geometry is IRaster)
                return GeometryRenderer.RenderRaster(feature.Geometry as IRaster, style, viewport);
            return null;
        }

        private static void PositionGeometry(Shape renderedGeometry, IReadOnlyViewport viewport, IStyle style, IFeature feature)
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
                GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.BoundingBox, viewport);
        }
    }
}