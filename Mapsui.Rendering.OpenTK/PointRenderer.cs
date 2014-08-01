using BruTile;
using BruTile.Web;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;
using All = OpenTK.Graphics.ES11.All;
using Bitmap = System.Drawing.Bitmap;
using GL = OpenTK.Graphics.ES11.GL;
using PixelFormat = OpenTK.Graphics.ES11.PixelFormat;
using PixelType = OpenTK.Graphics.ES11.PixelType;
using Point = Mapsui.Geometries.Point;
using TextureMagFilter = OpenTK.Graphics.ES11.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.ES11.TextureMinFilter;
using TextureParameterName = OpenTK.Graphics.ES11.TextureParameterName;
using TextureTarget = OpenTK.Graphics.ES11.TextureTarget;

namespace Mapsui.Rendering.OpenTK
{
    public class PointRenderer
    {
        private static BitmapData bitmapData;

        public static int LoadTexture(Stream data)
        {
            int texture;
            GL.ShadeModel(All.Smooth);
            GL.ClearColor(0, 0, 0, 1);

            GL.ClearDepth(1.0f);
            GL.Enable(All.DepthTest);
            GL.DepthFunc(All.Lequal);

            GL.Enable(All.CullFace);
            GL.CullFace(All.Front);

            GL.Hint(All.PerspectiveCorrectionHint, All.Nicest);

            // create texture ids
            GL.Enable(All.Texture2D);
            
            GL.GenTextures(1, out texture);

            GL.BindTexture(All.Texture2D, texture);
       
            data.Position = 0;
            var bitmap = (Bitmap)Image.FromStream(data);

            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, 0, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Rgba, PixelType.Bitmap, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.BindTexture(All.Texture2D, 0);
            //SetParameters();

            ////GL.MatrixMode.
            //Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);
            return texture;
        }
        
        private static void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameterx(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameterx(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
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
                        //GL.GenTextures(1, out _texture);


                        //var bitmap = (Bitmap)Image.FromStream(symbolStyle.Symbol.Data);
                        //var bitmapData = ToBitmapData(bitmap);

                        //GL.TexImage2D(TextureTarget.Texture2D, 0, 10, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Rgba, PixelType.Bitmap, bitmapData.Scan0);
                        //bitmap.UnlockBits(bitmapData);

                        //GL.BindTexture(TextureTarget.Texture2D, _texture);

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

            //var vectorStyle = style as VectorStyle;
            //if (vectorStyle != null)
            //{
            //    var fillColor = vectorStyle.Fill.Color;
            //    GL.Color4((byte)fillColor.R, (byte)fillColor.G, (byte)fillColor.B, (byte)fillColor.A);
            //    GL.PointSize((float)SymbolStyle.DefaultWidth);
            //    GL.EnableClientState(All.VertexArray);
            //    var destAsArray = new[] { (float)dest.X, (float)dest.Y};
            //    GL.VertexPointer(2, All.Float, 0, destAsArray);
            //    GL.DrawArrays(All.Points, 0, 1);
            //    GL.DisableClientState(All.VertexArray);
            //}
        }

        public static void RenderTexture(int textureId, float[] vertextArray)
        {
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, textureId);

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

            GL.DisableClientState(All.VertexArray);
            GL.DisableClientState(All.TextureCoordArray);
        }
    }
}
