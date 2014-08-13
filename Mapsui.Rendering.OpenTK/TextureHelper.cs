using System.IO;
using OpenTK.Graphics.ES11;

namespace Mapsui.Rendering.OpenTK
{
    public static class TextureHelper
    {
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

            GL.Enable(All.Blend); // Enables the alpha channel to be used in the color buffer
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

            GL.Disable(All.Blend);

            GL.DisableClientState(All.VertexArray);
            GL.DisableClientState(All.TextureCoordArray);
        }
    }
}
