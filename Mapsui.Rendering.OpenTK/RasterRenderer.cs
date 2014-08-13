using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using OpenTK.Graphics.ES11;
using System;
using System.Diagnostics;
using System.IO;

namespace Mapsui.Rendering.OpenTK
{
    public static class RasterRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;
                TextureInfo textureInfo;

                if (!feature.RenderedGeometry.ContainsKey(style))
                {
                    textureInfo = LoadTexture(raster.Data);
                    feature.RenderedGeometry[style] = textureInfo;
                }
                else
                {
                    textureInfo = (TextureInfo)feature.RenderedGeometry[style];
                }
                
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                RenderTexture(textureInfo.TextureId, ToVertexArray(RoundToPixel(destination)));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        
        public static TextureInfo LoadTexture(Stream data)
        {
            var textureInfo = new TextureInfo();

            GL.Enable(All.Texture2D);
            GL.GenTextures(1, out textureInfo.TextureId);
            GL.BindTexture(All.Texture2D, textureInfo.TextureId);
            
            SetParameters();

            TextureLoader.TexImage2D(data, out textureInfo.Width, out textureInfo.Height);

            GL.BindTexture(All.Texture2D, 0);

            return textureInfo;
        }

        private static void SetParameters()
        {
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
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

        public static void RenderTexture(TextureInfo textureInfo, float x, float y, float orientation = 0, float offsetX = 0, float offsetY = 0)
        {
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, textureInfo.TextureId);
            
            GL.PushMatrix();
            GL.Translate(x, y, 0f);
            GL.Rotate(orientation, 0, 0, 1);
            
            x = -offsetX; 
            y = -offsetY; 
            var halfWidth = textureInfo.Width / 2;
            var halfHeight = textureInfo.Height / 2;

            var vertextArray = new[]
                {
                    x - halfWidth, y - halfHeight,
                    x + halfWidth, y - halfHeight,
                    x + halfWidth, y + halfHeight,
                    x - halfWidth, y + halfHeight
                };

            RenderTextureWithoutBinding(textureInfo.TextureId, vertextArray);

            GL.PopMatrix();
            GL.BindTexture(All.Texture2D, 0);
            GL.Disable(All.Texture2D);
        }

        public static void RenderTexture(int textureId, float[] vertextArray)
        {
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, textureId);

            RenderTextureWithoutBinding(textureId, vertextArray);

            GL.BindTexture(All.Texture2D, 0);
            GL.Disable(All.Texture2D);
        }
        
        public static void RenderTextureWithoutBinding(int textureId, float[] vertextArray)
        {
            GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);
            
            GL.Enable(All.Blend); //Basically enables the alpha channel to be used in the color buffer
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha); //The operation/order to blend

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            
            var textureArray = new[]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f
            };

            GL.VertexPointer(2, All.Float, 0, vertextArray);
            GL.TexCoordPointer(2, All.Float, 0, textureArray);
            GL.DrawArrays(All.TriangleFan, 0, 4);
            
            GL.Disable(All.Blend); //Basically enables the alpha channel to be used in the color buffer

            GL.DisableClientState(All.VertexArray);
            GL.DisableClientState(All.TextureCoordArray);
        }
    }
}
