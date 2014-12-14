using Android.Graphics;
using Mapsui.Providers;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using Mapsui.Rendering.Android.ExtensionMethods;
using Bitmap = Android.Graphics.Bitmap;

namespace Mapsui.Rendering.Android
{
    static class PointRenderer
    {
        public static void Draw(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var dest = viewport.WorldToScreen(point);
            var symbolSize = (float)SymbolStyle.DefaultHeight;
            var symbolType = SymbolType.Ellipse;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.BitmapId >= 0)
                {
                    // Bitmap
                    if (!feature.RenderedGeometry.ContainsKey(style))
                        feature.RenderedGeometry[style] = 
                            BitmapFactory.DecodeStream(BitmapRegistry.Instance.Get(symbolStyle.BitmapId));
                    var bitmap = (Bitmap)feature.RenderedGeometry[style];
                    var halfWidth = bitmap.Width / 2;
                    var halfHeight = bitmap.Height / 2;
                    var dstRectForRender = new RectF((float)dest.X - halfWidth, (float)dest.Y - halfHeight, (float)dest.X + halfWidth, (float)dest.Y + halfHeight);
                    canvas.DrawBitmap(bitmap, null, dstRectForRender, null);
                    return;
                }
                symbolType = symbolStyle.SymbolType;
                if (symbolStyle.SymbolScale > 0) symbolSize = (float)symbolStyle.SymbolScale * symbolSize;
            }

            // Drawing
            var paints = style.ToAndroid();
            if (symbolType == SymbolType.Ellipse)
            {
                foreach (var paint in paints)
                {
                    canvas.DrawCircle((int)dest.X, (int)dest.Y, symbolSize, paint);
                    paint.Dispose();
                }
            }
            else
            {
                foreach (var paint in paints)
                {
                    canvas.DrawRect(-(float)SymbolStyle.DefaultWidth, (float)SymbolStyle.DefaultHeight, (float)SymbolStyle.DefaultWidth, -(float)SymbolStyle.DefaultHeight, paint);
                    paint.Dispose();
                }
            }
        }
    }
}