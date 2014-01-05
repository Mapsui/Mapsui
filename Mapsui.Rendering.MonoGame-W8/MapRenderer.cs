using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using BoundingBox = Mapsui.Geometries.BoundingBox;
using Color = Microsoft.Xna.Framework.Color;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.MonoGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MapRenderer
    {
        SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Color _backgroundColor = new Color(2, 5, 20);//new Color(27, 69, 127);
        private readonly IDictionary<Bitmap, Texture2D> _renderedResources = new Dictionary<Bitmap, Texture2D>();

        public MapRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        public void Draw(Map map, IViewport viewport)
        {
            if (_spriteBatch == null) _spriteBatch = new SpriteBatch(_graphicsDevice);

            _graphicsDevice.Clear(_backgroundColor);
            _spriteBatch.Begin();

            VisibleFeatureIterator.IterateLayers(viewport, map.Layers, RenderFeature);

            _spriteBatch.End();
        }

        public void Render(ILayer layer, IViewport viewport)
        {
            if (_spriteBatch == null) _spriteBatch = new SpriteBatch(_graphicsDevice);

            _graphicsDevice.Clear(Color.White);
            _spriteBatch.Begin();

            VisibleFeatureIterator.IterateLayer(viewport, layer, RenderFeature);

            _spriteBatch.End();
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                var raster = (feature.Geometry as IRaster);

                if (!feature.RenderedGeometry.Keys.Contains(style))
                {
                    feature.RenderedGeometry[style] = raster.Data.ToTexture2D(_graphicsDevice);
                }
                var bitmap = feature.RenderedGeometry[style] as Texture2D;
                if (bitmap == null) throw new Exception("Incorrect geometry type");
                var destination = RoundToPixel(WorldToScreen(viewport, raster.GetBoundingBox())).ToXna();
                _spriteBatch.Draw(bitmap, destination, bitmap.Bounds, Color.White);
            }
            if (feature.Geometry is Point)
            {
                if (style is VectorStyle) DrawPoint(viewport, style as VectorStyle, feature);
            }
        }

        private Texture2D CreateTextureFromStyleInfo(Brush brush, Pen pen, SymbolType symbolType)
        {
            const int width = (int)SymbolStyle.DefaultWidth;
            const int height = (int)SymbolStyle.DefaultHeight;
            var texture = new Texture2D(_graphicsDevice, width, height);
            var color = new Color(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
            var pixelColors = Enumerable.Range(0, width * height).Select(i => color).ToArray();
            texture.SetData(pixelColors);

            //PresentationParameters pp = _graphicsDevice.PresentationParameters;
            //var renderTarget = new RenderTarget2D(_graphicsDevice, 100, 100);

            //_graphicsDevice.SetRenderTarget(renderTarget);
            ////_graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            //using (var sprite = new SpriteBatch(_graphicsDevice))
            //{
            //    sprite.Begin();
            //    sprite.Draw(texture, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                
            //    sprite.End();
            //}

            //_graphicsDevice.SetRenderTarget(null);
            //var t = (Texture2D) renderTarget;
            return texture;
        }

        private void DrawPoint(IViewport viewport, VectorStyle vectorStyle, IFeature feature)
        {
            var symbolStyle = (vectorStyle is SymbolStyle)
                ? (vectorStyle as SymbolStyle)
                : new SymbolStyle
                    {
                        Fill = vectorStyle.Fill,
                        Line = vectorStyle.Line,
                        Outline = vectorStyle.Outline
                    };

            var destination = viewport.WorldToScreen(feature.Geometry as Point).ToXna();

            if (!feature.RenderedGeometry.ContainsKey(symbolStyle)) feature.RenderedGeometry[symbolStyle] = CreateTextureFromStyle(symbolStyle);

            var texture = (Texture2D)feature.RenderedGeometry[symbolStyle];

            var origin = new Vector2(
                texture.Width * 0.5f + (float)symbolStyle.SymbolOffset.X,
                texture.Height * 0.5f + (float)symbolStyle.SymbolOffset.Y);

            var rotationInRadians = (float)symbolStyle.SymbolRotation * Mapsui.Utilities.Constants.DegreesToRadians;

            _spriteBatch.Draw(texture, destination, null, Color.White * (float)symbolStyle.Opacity, rotationInRadians, 
                origin, (float)symbolStyle.SymbolScale, SpriteEffects.None, 0f);
        }

        private Texture2D CreateTextureFromStyle(SymbolStyle symbolStyle)
        {
            return symbolStyle.Symbol == null || symbolStyle.Symbol.Data == null
                       ? CreateTextureFromStyleInfo(symbolStyle.Fill, symbolStyle.Line, symbolStyle.SymbolType)
                       : CreateTextureFromBitmapSymbol(symbolStyle.Symbol); 
        }

        private Texture2D CreateTextureFromBitmapSymbol(Bitmap symbol)
        {
            return _renderedResources[symbol] = symbol.Data.ToTexture2D(_graphicsDevice);
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var box = new BoundingBox
            {
                Min = viewport.WorldToScreen(boundingBox.Min),
                Max = viewport.WorldToScreen(boundingBox.Max)
            };
            return box;
        }

        public static BoundingBox RoundToPixel(BoundingBox dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            return new BoundingBox(
                Math.Round(dest.Left),
                Math.Round(dest.Top),
                Math.Round(dest.Right),
                Math.Round(dest.Bottom));
        }
    }
}
