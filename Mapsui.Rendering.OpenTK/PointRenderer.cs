using Mapsui.Providers;
using Mapsui.Styles;
using OpenTK.Graphics.ES11;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.OpenTK
{
    public class PointRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var destination = viewport.WorldToScreen(point);

            if (style is SymbolStyle) DrawPointWithSymbolStyle(feature, (SymbolStyle)style, destination);
            else if (style is VectorStyle) DrawPointWithVectorStyle((VectorStyle)style, destination);
        }

        private static void DrawPointWithVectorStyle(VectorStyle vectorStyle, Point destination)
        {
            var color = vectorStyle.Fill.Color;
            GL.Color4((byte) color.R, (byte) color.G, (byte) color.B, (byte) color.A);
            GL.PointSize((float) SymbolStyle.DefaultWidth);
            GL.EnableClientState(All.VertexArray);
            var destAsArray = new[] {(float) destination.X, (float) destination.Y};
            GL.VertexPointer(2, All.Float, 0, destAsArray);
            GL.DrawArrays(All.Points, 0, 1);
            GL.DisableClientState(All.VertexArray);
        }

        private static void DrawPointWithSymbolStyle(IFeature feature, SymbolStyle symbolStyle, Point destination)
        {
            if (symbolStyle.Symbol != null && symbolStyle.Symbol.Data != null)
            {
                TextureInfo textureInfo;
                if (!feature.RenderedGeometry.ContainsKey(symbolStyle))
                {
                    textureInfo = TextureHelper.LoadTexture(symbolStyle.Symbol.Data);
                    feature.RenderedGeometry[symbolStyle] = textureInfo;
                }
                else
                {
                    textureInfo = (TextureInfo)feature.RenderedGeometry[symbolStyle];
                }
                TextureHelper.RenderTexture(textureInfo, (float)destination.X, (float)destination.Y, 
                    (float)symbolStyle.SymbolRotation, (float)symbolStyle.SymbolOffset.X, (float)symbolStyle.SymbolOffset.Y);
            }
        }
    }
}
