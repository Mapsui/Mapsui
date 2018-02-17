using Mapsui.Geometries.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI
{
    public partial class MapControl
    {
        /// <summary>
        /// Events
        /// </summary>
        
        /// <summary>
        /// TouchStart is called, when user press a mouse button or touch the display
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchStart;

        /// <summary>
        /// TouchEnd is called, when user release a mouse button or doesn't touch display anymore
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchEnd;

        /// <summary>
        /// TouchMove is called, when user move mouse over map (independent from mouse button state) or move finger on display
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchMove;

        /// <summary>
        /// Hover is called, when user move mouse over map without pressing mouse button
        /// </summary>
        public event EventHandler<HoverEventArgs> Hover;

        /// <summary>
        /// Swipe is called, when user release mouse button or lift finger while moving with a certain speed 
        /// </summary>
        public event EventHandler<SwipeEventArgs> Swipe;

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipeEventArgs> Fling;

        /// <summary>
        /// SingleTap is called, when user clicks with a mouse button or tap with a finger on map 
        /// </summary>
        public event EventHandler<TapEventArgs> SingleTap;

        /// <summary>
        /// LongTap is called, when user clicks with a mouse button or tap with a finger on map for 500 ms
        /// </summary>
        public event EventHandler<TapEventArgs> LongTap;

        /// <summary>
        /// DoubleTap is called, when user clicks with a mouse button or tap with a finger two or more times on map
        /// </summary>
        public event EventHandler<TapEventArgs> DoubleTap;

        /// <summary>
        /// Zoom is called, when map should be zoomed
        /// </summary>
        public event EventHandler<ZoomEventArgs> Zoom;

        /// <summary>
        /// Event handlers
        /// </summary>

        private bool HandleZoomOut(Geometries.Point screenPosition)
        {
            var args = new ZoomEventArgs(screenPosition, ZoomDirection.ZoomOut);

            Zoom?.Invoke(this, args);

            if (args.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool HandleZoomIn(Geometries.Point screenPosition)
        {
            var args = new ZoomEventArgs(screenPosition, ZoomDirection.ZoomIn);

            Zoom?.Invoke(this, args);

            if (args.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool HandleHover(Geometries.Point screenPosition)
        {
            var args = new HoverEventArgs(screenPosition);

            Hover?.Invoke(this, args);

            return args.Handled;
        }

        private bool HandleSwipe(double velocityX, double velocityY)
        {
            var args = new SwipeEventArgs(velocityX, velocityY);

            Swipe?.Invoke(this, args);

            // TODO
            // Perform standard behavior

            return args.Handled;
        }

        private bool HandleFling(double velocityX, double velocityY)
        {
            var args = new SwipeEventArgs(velocityX, velocityY);

            Fling?.Invoke(this, args);

            // TODO
            // Perform standard behavior

            return args.Handled;
        }

        private bool HandleTouchStart(List<Geometries.Point> touchPoints)
        {
            var args = new TouchEventArgs(touchPoints);

            TouchStart?.Invoke(this, args);

            if (args.Handled)
                return true;

            if (touchPoints.Count >= 2)
            {
                (_previousCenter, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                _mode = Zooming;
                _innerRotation = _map.Viewport.Rotation;
            }
            else
            {
                _mode = Dragging;
                _previousCenter = touchPoints.First();
            }

            return true;
        }

        private bool HandleTouchEnd(List<Geometries.Point> touchPoints, Geometries.Point releasedPoint)
        {
            var args = new TouchEventArgs(touchPoints);

            TouchEnd?.Invoke(this, args);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                InvalidateSurface();
                _mode = None;
                _map.ViewChanged(true);
            }

            return args.Handled;
        }

        private bool HandleTouchMove(List<Geometries.Point> touchPoints)
        {
            var args = new TouchEventArgs(touchPoints);

            TouchMove?.Invoke(this, args);

            if (args.Handled)
                return true;

            switch (_mode)
            {
                case Dragging:
                    {
                        if (touchPoints.Count != 1)
                            return false;

                        var touchPosition = touchPoints.First();

                        if (_previousCenter != null && !_previousCenter.IsEmpty())
                        {
                            _map.Viewport.Transform(touchPosition.X, touchPosition.Y, _previousCenter.X, _previousCenter.Y);

                            ViewportLimiter.LimitExtent(_map.Viewport, _map.PanMode, _map.PanLimits, _map.Envelope);

                            InvalidateSurface();
                        }

                        _previousCenter = touchPosition;
                    }
                    break;
                case Zooming:
                    {
                        if (touchPoints.Count < 2)
                            return false;

                        var (prevCenter, prevRadius, prevAngle) = (_previousCenter, _previousRadius, _previousAngle);
                        var (center, radius, angle) = GetPinchValues(touchPoints);

                        double rotationDelta = 0;

                        if (AllowPinchRotation)
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

                        InvalidateSurface();
                    }
                    break;
            }

            return true;
        }

        private bool HandleDoubleTap(Geometries.Point screenPosition, int numOfTaps)
        {
            var args = new TapEventArgs(screenPosition, numOfTaps);

            DoubleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var tapWasHandled = Map.InvokeInfo(screenPosition, screenPosition, _skiaScale, _renderer.SymbolCache, WidgetTouched, numOfTaps);

            if (!tapWasHandled)
            {
                // Double tap as zoom
                return HandleZoomIn(screenPosition);
            }

            return false;
        }

        private bool HandleSingleTap(Geometries.Point screenPosition)
        {
            var args = new TapEventArgs(screenPosition, 1);

            SingleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            return Map.InvokeInfo(screenPosition, screenPosition, _skiaScale, _renderer.SymbolCache, WidgetTouched, 1);
        }

        private bool HandleLongTap(Geometries.Point screenPosition)
        {
            var args = new TapEventArgs(screenPosition, 1);

            LongTap?.Invoke(this, args);

            return args.Handled;
        }

        /// <summary>
        /// Public functions
        /// </summary>
        
        public void Dispose()
        {
            Unsubscribe();
        }

        public void Unsubscribe()
        {
            UnsubscribeFromMapEvents(_map);
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

        /// <summary>
        /// Calculates center, radius and angle from a list of points
        /// </summary>
        /// <param name="screenPositions">List of points, normally touch points on display</param>
        /// <returns>
        /// center: Center of all points
        /// radius: Distance from center to first point of list
        /// angle: between first and second point of list
        /// </returns>
        private static (Geometries.Point centre, double radius, double angle) GetPinchValues(List<Geometries.Point> screenPositions)
        {
            if (screenPositions.Count < 2)
                throw new ArgumentException();

            double centerX = 0;
            double centerY = 0;

            foreach (var location in screenPositions)
            {
                centerX += location.X;
                centerY += location.Y;
            }

            centerX = centerX / screenPositions.Count;
            centerY = centerY / screenPositions.Count;

            var radius = Algorithms.Distance(centerX, centerY, screenPositions[0].X, screenPositions[0].Y);

            var angle = Math.Atan2(screenPositions[1].Y - screenPositions[0].Y, screenPositions[1].X - screenPositions[0].X) * 180.0 / Math.PI;

            return (new Geometries.Point(centerX, centerY), radius, angle);
        }
    }
}
