using System.Collections.Generic;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private readonly Game _game;
        private readonly IDictionary<string, Texture2D> _renderedResources = new Dictionary<string, Texture2D>(); 

        public MapRenderer(Game game)
        {
            _game = game;

            // this weird code is correct:
            new GraphicsDeviceManager(game);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        public void Draw(Map map, IViewport viewport)
        {
            if (_spriteBatch == null) _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            
            _game.GraphicsDevice.Clear(Color.LightGray);
            _spriteBatch.Begin();

            VisibleFeatureIterator.Render(viewport, map.Layers, RenderFeature);
            
            _spriteBatch.End();
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                var raster = (feature.Geometry as IRaster);
                var destination = ToXna(RoundToPixel(WorldToScreen(viewport, raster.GetBoundingBox())));
                var source = new Rectangle(0, 0, 256, 256);

                raster.Data.Position = 0;
                if (!feature.RenderedGeometry.Keys.Contains(new VectorStyle()))
                {
                    feature.RenderedGeometry[new VectorStyle()] = Texture2D.FromStream(_game.GraphicsDevice, raster.Data);
                }
                _spriteBatch.Draw(feature.RenderedGeometry[new VectorStyle()] as Texture2D, destination, source, Color.White);
            }
            if (feature.Geometry is Point)
            {
                if (style is SymbolStyle) DrawPoint(viewport, style as SymbolStyle, feature);
            }
        }

        private void DrawPoint(IViewport viewport, SymbolStyle style, IFeature feature)
        {
            var destination = ToXna(viewport.WorldToScreen(feature.Geometry as Point));

            if (!_renderedResources.ContainsKey(style.BitmapLocation))
            {
                var temp = new Texture2D(_game.GraphicsDevice, 1, 1);
                temp.SetData(new[] { Color.White });
                _renderedResources[style.BitmapLocation] = temp;
            }
            
            var texture = _renderedResources[style.BitmapLocation];
            
            _spriteBatch.Draw(texture,
                              destination,
                              null,
                              Color.Yellow,
                              0,
                              Vector2.Zero,
                              new Vector2(10, 10),
                              SpriteEffects.None,
                              0);

            const int width = 10;
            const int height = 10;
            var rect = new Rectangle((int) (destination.X - width*0.5), (int) (destination.Y - height*0.5),
                                     width, height);

            _spriteBatch.Draw(texture, rect, Color.Red);
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

        private static Rectangle ToXna(BoundingBox boundingBox)
        {
            return new Rectangle
            {
                X = (int)boundingBox.Left,
                Y = (int)boundingBox.Bottom,
                Width = (int)boundingBox.Width,
                Height = (int)boundingBox.Height
            };
        }

        private static Vector2 ToXna(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        public static BoundingBox RoundToPixel(BoundingBox dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            return  new BoundingBox(
                Math.Round(dest.Left),
                Math.Round(dest.Top),
                Math.Round(dest.Right),
                Math.Round(dest.Bottom));
        }

        public MemoryStream ToBitmapStream(double width, double height)
        {
            var renderTarget = new RenderTarget2D(_game.GraphicsDevice, (int)width, (int)height);
            _game.GraphicsDevice.SetRenderTarget(renderTarget);

            _game.GraphicsDevice.SetRenderTarget(null);
            var stream = new MemoryStream();
            renderTarget.SaveAsPng(stream, (int)width, (int)height);
            return stream;
        }
    }
}
