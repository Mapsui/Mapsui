using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
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
        private readonly Canvas tileCanvas = new Canvas();
        private readonly TileRenderer tileRenderer = new TileRenderer();

        public Canvas Canvas { get; private set; }

        public MapRenderer()
        {
            Canvas = new Canvas();
        }
        
        public void Render(IView view, Map map)
        {
            // The general approach to rendering witing the SilverlightRedering dll 
            // is to create a new canvas on every render iteration. 
            // This is very inefficient and should be rewritten one day.
            // For tileLayer we now use a workaround bu keeping the tiles
            // in a separate tileLayer canvas. 
            Canvas.Children.Clear();
            Canvas = new Canvas();
            Canvas.Children.Add(tileCanvas);
            foreach (var layer in map.Layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= view.Resolution &&
                    layer.MaxVisible >= view.Resolution)
                {
                    RenderLayer(view, layer);
                }
            }
            Canvas.Arrange(new System.Windows.Rect(0, 0, view.Width, view.Height));
        }
        
        private void RenderLayer(IView view, ILayer layer)
        {
            // Ideally I would like a solution where all rendering can be done through a single interface 
            // without the type check below.
            if (layer is LabelLayer)
            {
                var labelLayer = layer as LabelLayer;
                if (labelLayer.UseLabelStacking)
                    LabelRenderer.RenderStackedLabelLayer(Canvas, view, labelLayer);
                else
                    LabelRenderer.RenderLabelLayer(Canvas, view, labelLayer);
            }
            else if (layer is ITileLayer)
            {
                var tileLayer = (ITileLayer)layer;
                tileRenderer.Render(tileCanvas, tileLayer.Schema, view, tileLayer.MemoryCache);
            }
            else
            {
                RenderVectorLayer(Canvas, view, layer);
            }
        }

        private static void RenderVectorLayer(Canvas canvas, IView view, ILayer layer)
        {
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
        }

        private static void RenderGeometry(Canvas canvas, IView view, SharpMap.Styles.IStyle style, SharpMap.Providers.IFeature feature)
        {
            if (feature.Geometry is Point)
                canvas.Children.Add(GeometryRenderer.RenderPoint(feature.Geometry as Point, style, view));
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
                canvas.Children.Add(GeometryRenderer.RenderRaster(feature.Geometry as IRaster, style, view));
        }

        public Stream ToBitmapStream(double width, double height)
        {            
            Canvas.Arrange(new System.Windows.Rect(0, 0, width, height));

            #if !SILVERLIGHT
        
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new PixelFormat());
            renderTargetBitmap.Render(Canvas);
            var bitmap = new PngBitmapEncoder();
            bitmap.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            var bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream);
            
            #else

            var writeableBitmap = new WriteableBitmap((int)width, (int)height);
            writeableBitmap.Render(Canvas, null);
            var bitmapStream = Utilities.ConverToBitmapStream(writeableBitmap);

            #endif
            
            return bitmapStream;
        }
    }
}
