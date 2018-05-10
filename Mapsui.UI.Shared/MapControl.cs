using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

#if __ANDROID__
namespace Mapsui.UI.Android
#elif __IOS__
namespace Mapsui.UI.iOS
#elif __UWP__
namespace Mapsui.UI.Uwp
#elif __FORMS__
namespace Mapsui.UI
#else
namespace Mapsui.UI.Wpf
#endif
{
    public partial class MapControl
    {
        /// <summary>
        /// Private global variables
        /// </summary>
        
        /// <summary>
        /// Display scale for converting screen position to real position
        /// </summary>
        private float _scale;

        /// <summary>
        /// Mode of a touched move event
        /// </summary>
        private TouchMode _mode = TouchMode.None;

        /// <summary>
        /// Saver for center before last pinch movement
        /// </summary>
        private Geometries.Point _previousCenter = new Geometries.Point();

        /// <summary>
        /// Saver for angle before last pinch movement
        /// </summary>
        private double _previousAngle;

        /// <summary>
        /// Saver for radius before last pinch movement
        /// </summary>
        private double _previousRadius = 1f;

        /// <summary>
        /// Events
        /// </summary>

        /// <summary>
        /// TouchStart is called, when user press a mouse button or touch the display
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchStarted;

        /// <summary>
        /// TouchEnd is called, when user release a mouse button or doesn't touch display anymore
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchEnded;

        /// <summary>
        /// TouchMove is called, when user move mouse over map (independent from mouse button state) or move finger on display
        /// </summary>
#if __WPF__
        public new event EventHandler<TouchedEventArgs> TouchMove;
#else
        public event EventHandler<TouchedEventArgs> TouchMove;
#endif

        /// <summary>
        /// Hover is called, when user move mouse over map without pressing mouse button
        /// </summary>
#if __ANDROID__
        public new event EventHandler<HoveredEventArgs> Hovered;
#else
        public event EventHandler<HoveredEventArgs> Hovered;
#endif

        /// <summary>
        /// Swipe is called, when user release mouse button or lift finger while moving with a certain speed 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Swipe;

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Fling;

        /// <summary>
        /// SingleTap is called, when user clicks with a mouse button or tap with a finger on map 
        /// </summary>
        public event EventHandler<TappedEventArgs> SingleTap;

        /// <summary>
        /// LongTap is called, when user clicks with a mouse button or tap with a finger on map for 500 ms
        /// </summary>
        public event EventHandler<TappedEventArgs> LongTap;

        /// <summary>
        /// DoubleTap is called, when user clicks with a mouse button or tap with a finger two or more times on map
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTap;

        /// <summary>
        /// Zoom is called, when map should be zoomed
        /// </summary>
        public event EventHandler<ZoomedEventArgs> Zoomed;

        /// <summary>
        /// Properties
        /// </summary>

        /// <summary>
        /// Allow map panning through touch or mouse
        /// </summary>
        public bool PanLock { get; set; }

        /// <summary>
        /// Allow a rotation with a pinch gesture
        /// </summary>
        public bool RotationLock { get; set; }

        /// <summary>
        /// Allow zooming though touch or mouse
        /// </summary>
        public bool ZoomLock { get; set; }
        
        /// <summary>
        /// After how many degrees start rotation to take place
        /// </summary>
        public double UnSnapRotationDegrees { get; set; }

        /// <summary>
        /// With how many degrees from 0 should map snap to 0 degrees
        /// </summary>
        public double ReSnapRotationDegrees { get; set; }

        /// <summary>
        /// Event handlers
        /// </summary>

        /// <summary>
        /// Called, when map should zoom out
        /// </summary>
        /// <param name="screenPosition">Center of zoom out event</param>
        private bool OnZoomOut(Geometries.Point screenPosition)
        {
            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomOut);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        /// <summary>
        /// Called, when map should zoom in
        /// </summary>
        /// <param name="screenPosition">Center of zoom in event</param>
        private bool OnZoomIn(Geometries.Point screenPosition)
        {
            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomIn);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen hovers around
        /// </summary>
        /// <param name="screenPosition">Actual position of mouse/finger/pen</param>
        private bool OnHovered(Geometries.Point screenPosition)
        {
            var args = new HoveredEventArgs(screenPosition);

            Hovered?.Invoke(this, args);

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen swiped over map
        /// </summary>
        /// <param name="velocityX">Velocity in x direction in pixel/second</param>
        /// <param name="velocityY">Velocity in y direction in pixel/second</param>
        private bool OnSwiped(double velocityX, double velocityY)
        {
            var args = new SwipedEventArgs(velocityX, velocityY);

            Swipe?.Invoke(this, args);

            // TODO
            // Perform standard behavior

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen flinged over map
        /// </summary>
        /// <param name="velocityX">Velocity in x direction in pixel/second</param>
        /// <param name="velocityY">Velocity in y direction in pixel/second</param>
        private bool OnFlinged(double velocityX, double velocityY)
        {
            var args = new SwipedEventArgs(velocityX, velocityY);

            Fling?.Invoke(this, args);

            // TODO
            // Perform standard behavior

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen click/touch map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchStart(List<Geometries.Point> touchPoints)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchStarted?.Invoke(this, args);

            if (args.Handled)
                return true;

            if (touchPoints.Count >= 2)
            {
                (_previousCenter, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                _mode = TouchMode.Zooming;
                _innerRotation = _map.Viewport.Rotation;
            }
            else
            {
                _mode = TouchMode.Dragging;
                _previousCenter = touchPoints.First();
            }

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen anymore click/touch map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        /// <param name="releasedPoint">Released point, which was touched before</param>
        private bool OnTouchEnd(List<Geometries.Point> touchPoints, Geometries.Point releasedPoint)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchEnded?.Invoke(this, args);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                InvalidateCanvas();
                _mode = TouchMode.None;
                _map.ViewChanged(true);
            }

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen moves over map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchMove(List<Geometries.Point> touchPoints)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchMove?.Invoke(this, args);

            if (args.Handled)
                return true;

            switch (_mode)
            {
                case TouchMode.Dragging:
                    {
                        if (touchPoints.Count != 1)
                            return false;

                        var touchPosition = touchPoints.First();

                        if (_previousCenter != null && !_previousCenter.IsEmpty())
                        {
                            _map.Viewport.Transform(touchPosition.X, touchPosition.Y, _previousCenter.X, _previousCenter.Y);

                            ViewportLimiter.LimitExtent(_map.Viewport, _map.PanMode, _map.PanLimits, _map.Envelope);

                            InvalidateCanvas();
                        }

                        _previousCenter = touchPosition;
                    }
                    break;
                case TouchMode.Zooming:
                    {
                        if (touchPoints.Count < 2)
                            return false;

                        var (prevCenter, prevRadius, prevAngle) = (_previousCenter, _previousRadius, _previousAngle);
                        var (center, radius, angle) = GetPinchValues(touchPoints);

                        double rotationDelta = 0;

                        if (RotationLock)
                        {
                            _innerRotation += angle - prevAngle;
                            _innerRotation %= 360;

                            if (_innerRotation > 180)
                                _innerRotation -= 360;
                            else if (_innerRotation < -180)
                                _innerRotation += 360;

                            if (_map.Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                                rotationDelta = _innerRotation;
                            else if (_map.Viewport.Rotation != 0)
                            {
                                if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                                    rotationDelta = -_map.Viewport.Rotation;
                                else
                                    rotationDelta = _innerRotation - _map.Viewport.Rotation;
                            }
                        }

                        _map.Viewport.Transform(center.X, center.Y, prevCenter.X, prevCenter.Y, radius / prevRadius, rotationDelta);

                        (_previousCenter, _previousRadius, _previousAngle) = (center, radius, angle);

                        ViewportLimiter.Limit(_map.Viewport,
                            _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                            _map.PanMode, _map.PanLimits, _map.Envelope);

                        InvalidateCanvas();
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped on map 2 or more times
        /// </summary>
        /// <param name="screenPosition">First clicked/touched position on screen</param>
        /// <param name="numOfTaps">Number of taps on map (2 is a double click/tap)</param>
        private bool OnDoubleTapped(Geometries.Point screenPosition, int numOfTaps)
        {
            var args = new TappedEventArgs(screenPosition, numOfTaps);

            DoubleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var tapWasHandled = Map.InvokeInfo(screenPosition, screenPosition, _scale, _renderer.SymbolCache, WidgetTouched, numOfTaps);

            if (!tapWasHandled)
            {
                // Double tap as zoom
                return OnZoomIn(screenPosition);
            }

            return false;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped on map one time
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        private bool OnSingleTapped(Geometries.Point screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            SingleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            return Map.InvokeInfo(screenPosition, screenPosition, _scale, _renderer.SymbolCache, WidgetTouched, 1);
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped long on map
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        private bool OnLongTapped(Geometries.Point screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            LongTap?.Invoke(this, args);

            return args.Handled;
        }

        /// <summary>
        /// Public functions
        /// </summary>

#if !__WPF__ && !__UWP__
        // PDD: Disabled to get a compile
        //public new void Dispose()
        //{
        //    Unsubscribe();
        //    base.Dispose();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    Unsubscribe();
        //    base.Dispose(disposing);
        //}
#endif

        /// <summary>
        /// Unsubscribe from map events </summary>
        public void Unsubscribe()
        {
            UnsubscribeFromMapEvents(_map);
        }

        /// <summary>
        /// Converting function for world to screen
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in screen coordinates</returns>
        public Geometries.Point WorldToScreen(Geometries.Point worldPosition)
        {
            return WorldToScreen(Map.Viewport, _scale, worldPosition);
        }

        /// <summary>
        /// Converting function for screen to world
        /// </summary>
        /// <param name="screenPosition">Position in screen coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Geometries.Point ScreenToWorld(Geometries.Point screenPosition)
        {
            return ScreenToWorld(Map.Viewport, _scale, screenPosition);
        }

        /// <summary>
        /// Converting function for world to screen respecting scale
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="scale">Scale</param>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in screen coordinates</returns>
        public Point WorldToScreen(IViewport viewport, float scale, Point worldPosition)
        {
            var screenPosition = viewport.WorldToScreen(worldPosition);
            return new Point(screenPosition.X * scale, screenPosition.Y * scale);
        }

        /// <summary>
        /// Converting function for screen to world respecting scale
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="scale">Scale</param>
        /// <param name="screenPosition">Position in screen coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Point ScreenToWorld(IViewport viewport, float scale, Point screenPosition)
        {
            var worldPosition = viewport.ScreenToWorld(screenPosition.X * scale, screenPosition.Y * scale);
            return new Point(worldPosition.X, worldPosition.Y);
        }

        /// <summary>
        /// Private static functions
        /// </summary>
        
        private static (Geometries.Point centre, double radius, double angle) GetPinchValues(List<Geometries.Point> locations)
        {
            if (locations.Count < 2)
                throw new ArgumentException();

            double centerX = 0;
            double centerY = 0;

            foreach (var location in locations)
            {
                centerX += location.X;
                centerY += location.Y;
            }

            centerX = centerX / locations.Count;
            centerY = centerY / locations.Count;

            var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

            var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

            return (new Geometries.Point(centerX, centerY), radius, angle);
        }

        /// <summary>
        /// Private functions
        /// </summary>

        /// <summary>
        /// Subscribe to map events
        /// </summary>
        /// <param name="map">Map, to which events to subscribe</param>
        private void SubscribeToMapEvents(Map map)
        {
            map.DataChanged += MapDataChanged;
            map.PropertyChanged += MapPropertyChanged;
            map.RefreshGraphics += MapRefreshGraphics;
        }

        /// <summary>
        /// Unsubcribe from map events
        /// </summary>
        /// <param name="map">Map, to which events to unsubscribe</param>
        private void UnsubscribeFromMapEvents(Map map)
        {
            var temp = map;
            if (temp != null)
            {
                temp.DataChanged -= MapDataChanged;
                temp.PropertyChanged -= MapPropertyChanged;
                temp.RefreshGraphics -= MapRefreshGraphics;
                temp.AbortFetch();
            }
        }
    }
}
