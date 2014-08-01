using BruTile;
using BruTile.Web;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;
using All = OpenTK.Graphics.OpenGL.All;
using Bitmap = System.Drawing.Bitmap;
using GL = OpenTK.Graphics.OpenGL.GL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;
using Point = Mapsui.Geometries.Point;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;
using TextureParameterName = OpenTK.Graphics.OpenGL.TextureParameterName;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;

namespace Mapsui.Rendering.OpenTK
{
    public class PointRenderer
    {
        private static BitmapData _bitmapData;

        public static int LoadTexture(Stream data)
        {
            int texture;
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(0, 0, 0, 1);

            GL.ClearDepth(1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // create texture ids
            GL.Enable(EnableCap.Texture2D);
            
            GL.GenTextures(1, out texture);

            GL.BindTexture(TextureTarget.Texture2D, texture);
       
            data.Position = 0;
            var bitmap = (Bitmap)Image.FromStream(data);

            //bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, 0, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Rgba, PixelType.Bitmap, bitmapData.Scan0);
            //bitmap.UnlockBits(bitmapData);


            _bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmapData.Width, _bitmapData.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, _bitmapData.Scan0);
            bitmap.UnlockBits(_bitmapData);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            SetParameters();

            //Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);
            return texture;
        }
        
        private static void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
        }

        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var dest = viewport.WorldToScreen(point);
            var symbolSize = (float)SymbolStyle.DefaultHeight;
            var symbolType = SymbolType.Ellipse;
            int textureId;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.Symbol != null && symbolStyle.Symbol.Data != null)
                {

                    if (!feature.RenderedGeometry.ContainsKey(style))
                    {
                        textureId = LoadTexture(symbolStyle.Symbol.Data);
                        feature.RenderedGeometry[style] = textureId;
                    }
                    else
                    {
                        textureId = (int)feature.RenderedGeometry[style];
                    }


                    //var bitmap = (BitmapData)feature.RenderedGeometry[style];
                    //var halfWidth = bitmap.Width / 2;
                    //var halfHeight = bitmap.Height / 2;

                    var halfWidth = 32 / 2;
                    var halfHeight = 32 / 2;

                    var vertextArray = new[]
                    {
                        (float)dest.X - halfWidth, (float)dest.Y - halfHeight,
                        (float)dest.X + halfWidth, (float)dest.Y - halfHeight,
                        (float)dest.X + halfWidth, (float)dest.Y + halfHeight,
                        (float)dest.X - halfWidth, (float)dest.Y + halfHeight
                    };

                    RenderTexture(textureId, vertextArray);

                    //GL.Disable(EnableCap.Texture2D);
                    //var dstRectForRender = new RectF((float)dest.X - halfWidth, (float)dest.Y - halfHeight, (float)dest.X + halfWidth, (float)dest.Y + halfHeight);

                    //canvas.DrawBitmap(bitmap, null, dstRectForRender, null);
                    return;
                }
                symbolType = symbolStyle.SymbolType;
                if (symbolStyle.SymbolScale > 0) symbolSize = (float)symbolStyle.SymbolScale * symbolSize;
            }

            var vectorStyle = style as VectorStyle;
            if (vectorStyle != null)
            {
                var fillColor = vectorStyle.Fill.Color;
                GL.Color4((byte)fillColor.R, (byte)fillColor.G, (byte)fillColor.B, (byte)fillColor.A);
                GL.PointSize((float)SymbolStyle.DefaultWidth);
                GL.EnableClientState(ArrayCap.VertexArray);
                var destAsArray = new[] { (float)dest.X, (float)dest.Y };
                GL.VertexPointer(2, VertexPointerType.Float, 0, destAsArray);
                GL.DrawArrays(PrimitiveType.Points, 0, 1);
                GL.DisableClientState(ArrayCap.VertexArray);
            }
        }

        public static void RenderTexture(int textureId, float[] vertextArray)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);


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

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }
    }
}
