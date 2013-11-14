using BruTile.Web;
using Mapsui.Layers;
using Mapsui.Rendering.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI.MonoGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MapControl : Game
    {
        private MapRenderer _renderer;
        private readonly Map _map = new Map();
        private Viewport _viewport;
        private Vector2  _previousPosition;
        private TouchCollection? _previousTouches;
        private double _previousDistance;
        private double _previousScrollViewValue;
        
        public MapControl()
        {
            _map.Layers.Add(new TileLayer(new OsmTileSource()));

            // this weird code is correct:
            new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;
            base.Initialize();

            if (_viewport == null)
                if (CanInitializeView(_map, GraphicsDevice.Viewport))
                {
                    _viewport = InitializeView(_map, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
                    _renderer = new MapRenderer(GraphicsDevice);
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
            var center = GetCenter(touches);
            var distance = GetDistance(touches, center);
            double scale = 1;

            MouseState mouseState = Mouse.GetState();
            
            var scrollViewValue = mouseState.ScrollWheelValue;
            if (scrollViewValue == _previousScrollViewValue)
            {
                scale = GetScale(distance, _previousDistance);
            }
            else
            {
                if (scrollViewValue > _previousScrollViewValue) _viewport.Resolution *= 0.5;
                else _viewport.Resolution *= 2;
            }

            if (center != default(Vector2) && touches.Count > 0 &&
                _previousTouches != null &&
                touches.Count == _previousTouches.Value.Count)
            {
                _viewport.Transform(center.X, center.Y, _previousPosition.X, _previousPosition.Y, scale);
                _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
            }
            _previousPosition = center;
            _previousTouches = touches;
            _previousDistance = distance;
            _previousScrollViewValue = scrollViewValue;
            base.Update(gameTime);
        }

        private static double GetScale(double distance, double previousDistance)
        {
            return (distance <= 0) ? 1.0 : ((previousDistance <= 0) ? 1.0 : distance/previousDistance);
        }

        private double GetDistance(TouchCollection touches, Vector2 center)
        {
            if (touches.Count == 0) return 1;
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
            _renderer.Draw(_map, _viewport);
            base.Draw(gameTime);
        }

        private static bool CanInitializeView(Map map, Microsoft.Xna.Framework.Graphics.Viewport xnaViewport)
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
