﻿using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    class ImageStyleRenderer
    {
        public static void Draw(SKCanvas canvas, ImageStyle symbolStyle, Point destination,
                SymbolCache symbolCache, float opacity, double mapRotation)
        {
            if (symbolStyle.BitmapId < 0)
                return;

            var bitmap = symbolCache.GetOrCreate(symbolStyle.BitmapId);

            // Calc offset (relative or absolute)
            var offsetX = symbolStyle.SymbolOffset.IsRelative ? bitmap.Width * symbolStyle.SymbolOffset.X : symbolStyle.SymbolOffset.X;
            var offsetY = symbolStyle.SymbolOffset.IsRelative ? bitmap.Height * symbolStyle.SymbolOffset.Y : symbolStyle.SymbolOffset.Y;

            var rotation = (float)symbolStyle.SymbolRotation;
            if (symbolStyle.RotateWithMap) rotation += (float)mapRotation;

            switch (bitmap.Type)
            {
                case BitmapType.Bitmap:
                    BitmapRenderer.Draw(canvas, bitmap.Bitmap,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Svg:
                    SvgRenderer.Draw(canvas, bitmap.Svg,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Sprite:
                    var sprite = bitmap.Sprite;
                    if (sprite.Data == null)
                    {
                        var bitmapAtlas = symbolCache.GetOrCreate(sprite.Atlas);
                        sprite.Data = bitmapAtlas.Bitmap.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                            sprite.Y + sprite.Height));
                    }
                    BitmapRenderer.Draw(canvas, (SKImage)sprite.Data,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Picture:
                    PictureRenderer.Draw(canvas, bitmap.Picture,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
            }
        }
    }
}
