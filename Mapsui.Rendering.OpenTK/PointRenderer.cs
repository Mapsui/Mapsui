using System.Collections.Generic;
using System.Linq;
using Mapsui.Providers;
using Mapsui.Styles;
using OpenTK.Graphics.ES11;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.OpenTK
{
    public class PointRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature, IDictionary<int, TextureInfo> bitmapCache)
        {
            var point = feature.Geometry as Point;
            var destination = viewport.WorldToScreen(point);

            if (style is LabelStyle)
            {
                var labelStyle = (LabelStyle) style;
                LabelRenderer.Draw(labelStyle, labelStyle.GetLabelText(feature), (float)destination.X, (float)destination.Y);
            }
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null && symbolStyle.BitmapId >= 0) DrawPointWithSymbolStyle(symbolStyle, destination, bitmapCache);
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

        private static void DrawPointWithSymbolStyle(SymbolStyle symbolStyle, Point destination, IDictionary<int, TextureInfo> bitmapCache)
        {
            TextureInfo textureInfo;
            if (!bitmapCache.Keys.Contains(symbolStyle.BitmapId))
            {
                textureInfo = TextureHelper.LoadTexture(BitmapRegistry.Instance.Get(symbolStyle.BitmapId));
                bitmapCache[symbolStyle.BitmapId] = textureInfo;
            }
            else
            {
                textureInfo = bitmapCache[symbolStyle.BitmapId];
            }

            TextureHelper.RenderTexture(textureInfo, (float)destination.X, (float)destination.Y, 
                (float)symbolStyle.SymbolRotation, 
                (float)symbolStyle.SymbolOffset.X, (float)symbolStyle.SymbolOffset.Y, 
                opacity:(float)symbolStyle.Opacity, scale:(float)symbolStyle.SymbolScale);
        }
    }
}
