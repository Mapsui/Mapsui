using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.UI
{
    public class SkiaControl : FrameworkElement
    {
        public Map Map { get; set; }
        private readonly MapRenderer _renderer = new MapRenderer();

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource == null) throw new Exception("PresentationSource is null");
            var compositionTarget = presentationSource.CompositionTarget;
            if (compositionTarget == null) throw new Exception("CompositionTarget is null");

            var m = compositionTarget.TransformToDevice;
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
                    if (!double.IsNaN(Map.Viewport.Resolution))
                    {
                        _renderer.Render(skcanvas, Map.Viewport, Map.Layers, Map.BackColor);
                    }
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
        }
    }
}