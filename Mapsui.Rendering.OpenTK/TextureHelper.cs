using System.IO;
using Mapsui.Styles;
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

            PlatformTextureLoader.TexImage2D(data, out textureInfo.Width, out textureInfo.Height);

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

        public static void RenderTexture(TextureInfo textureInfo, float x, float y, float orientation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            float opacity = 1f,
            float scale = 1f)
        {
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, textureInfo.TextureId);

            GL.PushMatrix();
            GL.Translate(x, y, 0f);
            GL.Rotate(orientation, 0, 0, 1);
            GL.Scale (scale, scale, 1);

            x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, textureInfo.Width);
            y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, textureInfo.Height);

            var halfWidth = textureInfo.Width / 2;
            var halfHeight = textureInfo.Height / 2;

            var vertextArray = new[]
            {
                x - halfWidth, y - halfHeight,
                x + halfWidth, y - halfHeight,
                x + halfWidth, y + halfHeight,
                x - halfWidth, y + halfHeight
            };

            RenderTextureWithoutBinding(textureInfo.TextureId, vertextArray, opacity);

            GL.PopMatrix();
            GL.BindTexture(All.Texture2D, 0);
            GL.Disable(All.Texture2D);
        }

        private static int DetermineHorizontalAlignmentCorrection(LabelStyle.HorizontalAlignmentEnum horizontalAlignment,
            int width)
        {
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width / 2;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -width / 2;
            return 0; // center
        }

        private static int DetermineVerticalAlignmentCorrection(LabelStyle.VerticalAlignmentEnum verticalAlignment,
            int height)
        {
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -height / 2;
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height / 2;
            return 0; // center
        }

        public static void RenderTexture(int textureId, float[] vertextArray)
        {
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, textureId);

            RenderTextureWithoutBinding(textureId, vertextArray);

            GL.BindTexture(All.Texture2D, 0);
            GL.Disable(All.Texture2D);
        }

        static readonly float[] TextureArray =
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f
            };

        public static void RenderTextureWithoutBinding(int textureId, float[] vertextArray, float opacity = 1f)
        {
            GL.Color4((byte)255, (byte)255, (byte)255, (byte)(255 * opacity));

            GL.Enable(All.Blend); // Enables the alpha channel to be used in the color buffer
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha); //The operation/order to blend

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);

            GL.VertexPointer(2, All.Float, 0, vertextArray);
            GL.TexCoordPointer(2, All.Float, 0, TextureArray);
            GL.DrawArrays(All.TriangleFan, 0, 4);

            GL.Disable(All.Blend);

            GL.DisableClientState(All.VertexArray);
            GL.DisableClientState(All.TextureCoordArray);
        }
    }
}
