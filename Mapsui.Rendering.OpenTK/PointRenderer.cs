using Mapsui.Providers;
using Mapsui.Styles;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.OpenTK
{
    public class PointRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            var point = feature.Geometry as Point;
            var dest = viewport.WorldToScreen(point);
            var symbolSize = (float)SymbolStyle.DefaultHeight;
            var symbolType = SymbolType.Ellipse;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.Symbol != null && symbolStyle.Symbol.Data != null)
                {
                    int textureId;
                    if (!feature.RenderedGeometry.ContainsKey(style))
                    {
                        textureId = RasterRenderer.LoadTexture(symbolStyle.Symbol.Data);
                        feature.RenderedGeometry[style] = textureId;
                    }
                    else
                    {
                        textureId = (int)feature.RenderedGeometry[style];
                    }

                    RasterRenderer.RenderTexture(textureId, (float)dest.X, (float)dest.Y);
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

     
    }
}
