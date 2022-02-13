using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal class ImageStyleRenderer
    {
        public static void Draw(SKCanvas canvas, ImageStyle symbolStyle, double x, double y,
                ISymbolCache symbolCache, float opacity, double mapRotation)
        {
            if (symbolStyle.BitmapId < 0)
                return;

            var bitmap = (BitmapInfo)symbolCache.GetOrCreate(symbolStyle.BitmapId);
            if (bitmap == null)
                return;

            // Calc offset (relative or absolute)
            var offsetX = symbolStyle.SymbolOffset.IsRelative ? bitmap.Width * symbolStyle.SymbolOffset.X : symbolStyle.SymbolOffset.X;
            var offsetY = symbolStyle.SymbolOffset.IsRelative ? bitmap.Height * symbolStyle.SymbolOffset.Y : symbolStyle.SymbolOffset.Y;

            var rotation = (float)symbolStyle.SymbolRotation;
            if (symbolStyle.RotateWithMap) rotation += (float)mapRotation;

            switch (bitmap.Type)
            {
                case BitmapType.Bitmap:
                    if (bitmap.Bitmap == null)
                        return;

                    BitmapRenderer.Draw(canvas, bitmap.Bitmap,
                        (float)x, (float)y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Picture:
                    if (bitmap.Picture == null)
                        return;

                    PictureRenderer.Draw(canvas, bitmap.Picture,
                        (float)x, (float)y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Svg:
                    if (bitmap.Svg == null)
                        return;

                    SvgRenderer.Draw(canvas, bitmap.Svg,
                        (float)x, (float)y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Sprite:
                    if (bitmap.Sprite == null)
                        return;

                    var sprite = bitmap.Sprite;
                    if (sprite.Data == null)
                    {
                        var bitmapAtlas = (BitmapInfo)symbolCache.GetOrCreate(sprite.Atlas);
                        sprite.Data = bitmapAtlas?.Bitmap?.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                            sprite.Y + sprite.Height));
                    }
                    if (sprite.Data is SKImage skImage)
                        BitmapRenderer.Draw(canvas, skImage,
                            (float)x, (float)y,
                            rotation,
                            (float)offsetX, (float)offsetY,
                            opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
            }
        }
    }
}
