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
            var dest = viewport.WorldToScreen(point);
            var symbolSize = (float)SymbolStyle.DefaultHeight;
            var symbolType = SymbolType.Ellipse;

            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null)
            {
                if (symbolStyle.Symbol != null && symbolStyle.Symbol.Data != null)
                {
                    TextureInfo textureInfo;
                    if (!feature.RenderedGeometry.ContainsKey(style))
                    {
                        textureInfo = TextureHelper.LoadTexture(symbolStyle.Symbol.Data);
                        feature.RenderedGeometry[style] = textureInfo;
                    }
                    else
                    {
                        textureInfo = (TextureInfo)feature.RenderedGeometry[style];
                    }

                    TextureHelper.RenderTexture(textureInfo, (float)dest.X, (float)dest.Y, (float)symbolStyle.SymbolRotation, (float)symbolStyle.SymbolOffset.X, (float)symbolStyle.SymbolOffset.Y);
                    return;
                }
                symbolType = symbolStyle.SymbolType;
                if (symbolStyle.SymbolScale > 0) symbolSize = (float)symbolStyle.SymbolScale * symbolSize;
            }

            var vectorStyle = style as VectorStyle;
            if (vectorStyle != null)
            {
                var color = vectorStyle.Fill.Color;
                GL.Color4((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
                GL.PointSize((float)SymbolStyle.DefaultWidth);
                GL.EnableClientState(All.VertexArray);
                var destAsArray = new[] { (float)dest.X, (float)dest.Y };
                GL.VertexPointer(2, All.Float, 0, destAsArray);
                GL.DrawArrays(All.Points, 0, 1);
                GL.DisableClientState(All.VertexArray);
            }
        }
    }
}
