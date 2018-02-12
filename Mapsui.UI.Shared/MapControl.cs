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
        
        public event EventHandler<TouchEventArgs> TouchStart;
        public event EventHandler<TouchEventArgs> TouchEnd;
        public event EventHandler<TouchEventArgs> TouchMove;
        public event EventHandler<HoverEventArgs> Hover;
        public event EventHandler<SwipeEventArgs> Swipe;
        public event EventHandler<SwipeEventArgs> Fling;
        public event EventHandler<TapEventArgs> SingleTap;
        public event EventHandler<TapEventArgs> LongTap;
        public event EventHandler<TapEventArgs> DoubleTap;
        public event EventHandler<ZoomEventArgs> Zoom;

        /// <summary>
        /// Event handlers
        /// </summary>

        private bool HandleZoomOut(Geometries.Point location)
        {
            var handler = Zoom;
            var eventArgs = new ZoomEventArgs(location, ZoomDirection.ZoomOut, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool HandleZoomIn(Geometries.Point location)
        {
            var handler = Zoom;
            var eventArgs = new ZoomEventArgs(location, ZoomDirection.ZoomIn, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool HandleHover(Geometries.Point location)
        {
            var handler = Hover;
            var eventArgs = new HoverEventArgs(location, false);

            handler?.Invoke(this, eventArgs);

            return eventArgs.Handled;
        }

        private bool HandleSwipe(double velocityX, double velocityY)
        {
            var handler = Swipe;
            var eventArgs = new SwipeEventArgs(velocityX, velocityY, false);

            handler?.Invoke(this, eventArgs);

            // TODO
            // Perform standard behavior

            return eventArgs.Handled;
        }

        private bool HandleFling(double velocityX, double velocityY)
        {
            var handler = Fling;
            var eventArgs = new SwipeEventArgs(velocityX, velocityY, false);

            handler?.Invoke(this, eventArgs);

            // TODO
            // Perform standard behavior

            return eventArgs.Handled;
        }

        private bool HandleTouchStart(List<Geometries.Point> touchPoints)
        {
            var handler = TouchStart;
            var eventArgs = new TouchEventArgs(touchPoints, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
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
            var handler = TouchEnd;
            var eventArgs = new TouchEventArgs(touchPoints, false);

            handler?.Invoke(this, eventArgs);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                InvalidateSurface();
                _mode = None;
                _map.ViewChanged(true);
            }

            return eventArgs.Handled;
        }

        private bool HandleTouchMove(List<Geometries.Point> touchPoints)
        {
            var handler = TouchMove;
            var eventArgs = new TouchEventArgs(touchPoints, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
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

        private bool HandleDoubleTap(Geometries.Point location, int numOfTaps)
        {
            var handler = DoubleTap;
            var eventArgs = new TapEventArgs(location, numOfTaps, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            var tapWasHandled = Map.InvokeInfo(location, location, _skiaScale, _renderer.SymbolCache, WidgetTouched, numOfTaps);

            if (!tapWasHandled)
            {
                // Double tap as zoom
                return HandleZoomIn(location);
            }

            return false;
        }

        private bool HandleSingleTap(Geometries.Point location)
        {
            var handler = SingleTap;
            var eventArgs = new TapEventArgs(location, 1, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            return Map.InvokeInfo(location, location, _skiaScale, _renderer.SymbolCache, WidgetTouched, 1);
        }

        private bool HandleLongTap(Geometries.Point location)
        {
            var handler = LongTap;
            var eventArgs = new TapEventArgs(location, 1, false);

            handler?.Invoke(this, eventArgs);

            return eventArgs.Handled;
        }
    }
}
