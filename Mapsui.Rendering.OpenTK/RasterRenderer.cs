using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering.OpenTK
{
    public static class RasterRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature, IDictionary<object, TextureInfo> TextureCache, long currentIteration)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;

                TextureInfo textureInfo;

                if (!TextureCache.Keys.Contains(raster))
                {
                    textureInfo = TextureHelper.LoadTexture(raster.Data);
                    TextureCache[raster] = textureInfo;
                }
                else
                {
                    textureInfo = TextureCache[raster];
                }

                textureInfo.IterationUsed = currentIteration;
                TextureCache[raster] = textureInfo;
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                TextureHelper.RenderTexture(textureInfo.TextureId, ToVertexArray(RoundToPixel(destination)));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
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
