using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class SkiaControl : FrameworkElement
    {
        public Map Map { get; set; }
        private MapRenderer _renderer = new MapRenderer();

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            var dpiX = m.M11;
            var dpiY = m.M22;

            var width = (int)(ActualWidth * dpiX);
            var height = (int)(ActualHeight * dpiY);

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            bitmap.Lock();
            using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, 
                bitmap.BackBuffer, bitmap.BackBufferStride))
            {
                var skcanvas = surface.Canvas;
                skcanvas.Scale((float)dpiX, (float)dpiY);
                using (new SKAutoCanvasRestore(skcanvas, true))
                {
                    //!!!PolygonWithHole(skcanvas, (int)ActualWidth, (int)ActualHeight);
                    _renderer.SKCanvas = skcanvas;
                    if (!double.IsNaN(Map.Viewport.Resolution)) _renderer.Render(Map.Viewport, Map.Layers);
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
        }

        public static void PolygonWithHole(SKCanvas canvas, int width, int height)
        {
            using (var path = new SKPath())
            {
                // outer ring
                path.MoveTo(-100, -100);
                path.LineTo(-100, 100);
                path.LineTo(100, 100);
                path.LineTo(100, -100);

                // innner ring
                path.MoveTo(-50, -50);
                path.LineTo(50, -50);
                path.LineTo(50, 50);
                path.LineTo(-50, 50);

                path.Close();

                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    canvas.Clear(SKColors.White);
                    canvas.Translate(width / 2f, height / 2f);
                    canvas.DrawPath(path, paint);
                }
            }
        }
    }
}
