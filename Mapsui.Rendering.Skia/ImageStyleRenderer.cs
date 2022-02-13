using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal class ImageStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
        {
            switch (feature)
            {
                case (PointFeature pointFeature):
                    DrawPointFeature(canvas, viewport, layer, feature, style, symbolCache, iteration);
                    break;
                case (GeometryFeature geometryFeatureNts):
                    switch (geometryFeatureNts.Geometry)
                    {
                        case GeometryCollection collection:
                            for (var i = 0; i < collection.NumGeometries; i++)
                                Draw(canvas, viewport, layer, new GeometryFeature(collection.GetGeometryN(i)), style, symbolCache, iteration);
                            break;
                        case Point point:
                            Draw(canvas, viewport, layer, new PointFeature(point.X, point.Y), style, symbolCache, iteration);
                            break;
                    }
                    break;
                case (GeometryCollection geometryFeatureCollection):
                    for (var i = 0; i < geometryFeatureCollection.NumGeometries; i++)
                        Draw(canvas, viewport, layer, new GeometryFeature(geometryFeatureCollection.GetGeometryN(i)), style, symbolCache, iteration);
                    break;
            }

            return true;
        }

        public bool DrawPointFeature(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
        {
            var pointFeature = (PointFeature)feature;
            var imageStyle = (ImageStyle)style;
            var opacity = (float)(layer.Opacity * style.Opacity);

            var (destX, destY) = viewport.WorldToScreenXY(pointFeature.Point.X, pointFeature.Point.Y);

            if (imageStyle.BitmapId < 0)
                return false;

            var bitmap = (BitmapInfo)symbolCache.GetOrCreate(imageStyle.BitmapId);
            if (bitmap == null)
                return false;

            // Calc offset (relative or absolute)
            var offsetX = imageStyle.SymbolOffset.IsRelative ? bitmap.Width * imageStyle.SymbolOffset.X : imageStyle.SymbolOffset.X;
            var offsetY = imageStyle.SymbolOffset.IsRelative ? bitmap.Height * imageStyle.SymbolOffset.Y : imageStyle.SymbolOffset.Y;

            var rotation = (float)imageStyle.SymbolRotation;
            if (imageStyle.RotateWithMap) rotation += (float)viewport.Rotation;

            switch (bitmap.Type)
            {
                case BitmapType.Bitmap:
                    if (bitmap.Bitmap == null)
                        return false;

                    BitmapRenderer.Draw(canvas, bitmap.Bitmap,
                        (float)destX, (float)destY,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)imageStyle.SymbolScale);
                    break;
                case BitmapType.Picture:
                    if (bitmap.Picture == null)
                        return false;

                    PictureRenderer.Draw(canvas, bitmap.Picture,
                        (float)destX, (float)destY,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)imageStyle.SymbolScale);
                    break;
                case BitmapType.Svg:
                    if (bitmap.Svg == null)
                        return false;

                    SvgRenderer.Draw(canvas, bitmap.Svg,
                        (float)destX, (float)destY,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)imageStyle.SymbolScale);
                    break;
                case BitmapType.Sprite:
                    if (bitmap.Sprite == null)
                        return false;

                    var sprite = bitmap.Sprite;
                    if (sprite.Data == null)
                    {
                        var bitmapAtlas = (BitmapInfo)symbolCache.GetOrCreate(sprite.Atlas);
                        sprite.Data = bitmapAtlas?.Bitmap?.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                            sprite.Y + sprite.Height));
                    }
                    if (sprite.Data is SKImage skImage)
                        BitmapRenderer.Draw(canvas, skImage,
                            (float)destX, (float)destY,
                            rotation,
                            (float)offsetX, (float)offsetY,
                            opacity: opacity, scale: (float)imageStyle.SymbolScale);
                    break;
            }

            return true;
        }
    }
}
