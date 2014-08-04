using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
#if ES11
using OpenTK.Graphics.ES11;
using ArrayCapX = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Graphics.OpenGL;
using ArrayCapX = OpenTK.Graphics.OpenGL.ArrayCap;
#endif

namespace Mapsui.Rendering.OpenTK
{
    struct RectF
    {
        public RectF(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;
    }

    public static class RasterRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;
                if (!feature.RenderedGeometry.ContainsKey(style))
                {
                    var textureId = LoadTexture(raster.Data);
                    feature.RenderedGeometry[style] = textureId;
                }
                
                var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                dest = new BoundingBox(
                    dest.MinX,
                    dest.MinY,
                    dest.MaxX,
                    dest.MaxY);

                var destination = RoundToPixel(dest);

                RenderTexture((int)feature.RenderedGeometry[style], ToVertexArray(destination));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static int LoadTexture(Stream data)
        {
            int texture;

            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            SetParameters();

            data.Position = 0;
            var bitmap = (System.Drawing.Bitmap)Image.FromStream(data);
            //bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, 0, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Rgba, PixelType.Bitmap, bitmapData.Scan0);
            //bitmap.UnlockBits(bitmapData);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, 0, bitmapData.Width, bitmapData.Height, 0, 0, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

        private static void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
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

        private static RectF RoundToPixel(BoundingBox dest)
        {
            return new RectF(
                (float)Math.Round(dest.Left),
                (float)Math.Round(Math.Min(dest.Top, dest.Bottom)),
                (float)Math.Round(dest.Right),
                (float)Math.Round(Math.Max(dest.Top, dest.Bottom)));
        }

        private static float[] ToVertexArray(RectF rect)
        {
            return new[]
            {
                rect.MinX, rect.MinY,
                rect.MaxX, rect.MinY,
                rect.MaxX, rect.MaxY,
                rect.MinX, rect.MaxY
            };
        }

        //private static AndroidBitmap ToAndroidBitmap(IGeometry geometry)
        //{
        //    var raster = (IRaster)geometry;
        //    var rasterData = raster.Data.ToArray();
        //    var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
        //    return bitmap;
        //}

        public static void RenderTexture(int textureId, float x, float y)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            int width = 32;
            //GL.GetTexLevelParameter(TextureTarget.Texture2D,0,  GetTextureParameter.TextureWidth, out width);
            int height = 32;
            //GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out height);

            x = (float)Math.Round(x);
            y = (float)Math.Round(y);
            var halfWidth = width / 2;
            var halfHeight = height / 2;

            var vertextArray = new[]
                {
                    x - halfWidth, y - halfHeight,
                    x + halfWidth, y - halfHeight,
                    x + halfWidth, y + halfHeight,
                    x - halfWidth, y + halfHeight
                };

            RenderTextureWithoutBinding(textureId, vertextArray);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
        }

        public static void RenderTexture(int textureId, float[] vertextArray)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            RenderTextureWithoutBinding(textureId, vertextArray);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
        }

        public static void RenderTextureWithoutBinding(int textureId, float[] vertextArray)
        {
            GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);

            //Basically enables the alpha channel to be used in the color buffer
            GL.Enable(EnableCap.Blend);
            //The operation/order to blend
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.EnableClientState(ArrayCapX.VertexArray);
            GL.EnableClientState(ArrayCapX.TextureCoordArray);
            
            var textureArray = new[]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f
            };

            GL.VertexPointer(2, VertexPointerType.Float, 0, vertextArray);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, textureArray);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            //Basically enables the alpha channel to be used in the color buffer
            GL.Disable(EnableCap.Blend);
            //The operation/order to blend
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, 0);

            GL.DisableClientState(ArrayCapX.VertexArray);
            GL.DisableClientState(ArrayCapX.TextureCoordArray);
        }
    }
}
