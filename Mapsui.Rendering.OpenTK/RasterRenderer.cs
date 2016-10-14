using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering.OpenTK
{
    public static class RasterRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature, 
            IDictionary<object, TextureInfo> textureCache, long currentIteration)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;

                TextureInfo textureInfo;

                if (!textureCache.Keys.Contains(raster))
                {
                    textureInfo = TextureHelper.LoadTexture(raster.Data);
                    textureCache[raster] = textureInfo;
                }
                else
                {
                    textureInfo = textureCache[raster];
                }

                textureInfo.IterationUsed = currentIteration;
                textureCache[raster] = textureInfo;
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                TextureHelper.RenderTexture(textureInfo.TextureId, ToVertexArray(RoundToPixel(destination)));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
                (
                    Math.Min(first.X, second.X),
                    Math.Min(first.Y, second.Y),
                    Math.Max(first.X, second.X),
                    Math.Max(first.Y, second.Y)
                );
        }

        private static BoundingBox RoundToPixel(BoundingBox boundingBox)
        {
            return new BoundingBox(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }

        private static float[] ToVertexArray(BoundingBox boundingBox)
        {
            return new[]
            {
                (float)boundingBox.MinX, (float)boundingBox.MinY,
                (float)boundingBox.MaxX, (float)boundingBox.MinY,
                (float)boundingBox.MaxX, (float)boundingBox.MaxY,
                (float)boundingBox.MinX, (float)boundingBox.MaxY
            };
        }
    }
}
