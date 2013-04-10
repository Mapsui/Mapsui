using System;
using System.Collections.Generic;
using System.Linq;
using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Rendering.MonoGame_W8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Point = Microsoft.Xna.Framework.Point;

namespace Mapsui.UI.MonoGame_W8
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MapControl : Game
    {
        MapRenderer _renderer;
        private Map _map = new Map();
        private Viewport _viewport;
        private Vector2  _previousPosition;
        private TouchCollection? _previousTouches;
        private int _touchCount;
        private double _previousDistance;
        
        public MapControl()
        {
            _renderer = new MapRenderer(this);
            _map.Layers.Add(new TileLayer(new OsmTileSource()));
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;
            base.Initialize();

            if (_viewport == null)
                if (CanInitializeView(_map, GraphicsDevice.Viewport))
                {
                    _viewport = InitializeView(_map, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
                }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var touches = TouchPanel.GetState();

            var pos = GetCenter(touches);
            var distance = GetScale(touches);

            var scale = (distance <= 0) ? 1.0 : ((_previousDistance <= 0) ? 1.0 : distance/_previousDistance);
            
            if (pos != default(Vector2) && _touchCount > 0 &&
                _previousTouches != null &&
                touches.Count == _previousTouches.Value.Count)
            {
                _viewport.Transform(pos.X, pos.Y, _previousPosition.X, _previousPosition.Y, scale);
                _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
            }
            _previousPosition = pos;
            _previousTouches = touches;
            _previousDistance = distance;
            _touchCount = touches.Count;
            base.Update(gameTime);
        }

        private double GetScale(TouchCollection touches)
        {
            if (touches.Count == 0) return 1;

            var center = GetCenter(touches);

            float y = 0;

            float distance = touches.Sum(touch => Vector2.Distance(center, touch.Position));

            return distance / touches.Count;
        }



        private Vector2 GetCenter(IList<TouchLocation> touches)
        {
            if (touches.Count == 0) return new Vector2(0.0f);

            float x = 0;
            float y = 0;

            foreach (var touch in touches)
            {
                x += touch.Position.X;
                y += touch.Position.Y;
            }
            return new Vector2(x / touches.Count, y / touches.Count);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (_viewport == null) return;
            _renderer.Draw(_map, _viewport, gameTime);
            base.Draw(gameTime);
        }

        private bool CanInitializeView(Map map, Microsoft.Xna.Framework.Graphics.Viewport xnaViewport)
        {
            if (xnaViewport.Width == 0) return false;
            if (xnaViewport.Height == 0) return false;
            if (map == null) return false;
            if (map.Envelope == null) return false;
            if (map.Envelope.Width.IsNanOrZero()) return false;
            if (map.Envelope.Height.IsNanOrZero()) return false;
            if (map.Envelope.GetCentroid() == null) return false;
            return true;
        }

        private static Viewport InitializeView(Map map, double screenWidth, double screenHeight)
        {
            var resultion = ((map.Envelope.Width/map.Envelope.Height) > (screenWidth/screenHeight))
                                ? map.Envelope.Width/screenWidth
                                : map.Envelope.Height/screenHeight;

            return new Viewport
                {
                    Center = map.Envelope.GetCentroid(),
                    Resolution = resultion,
                    Width = screenWidth,
                    Height = screenHeight
                };
        }

        
    }
}
