using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using Mapsui.Logging;
using Mapsui.Widgets;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SkiaSharp;
using System.Threading;

namespace Mapsui.UI.Forms
{
    public class MapControl : SKCanvasView, IMapControl, IDisposable
    {
        class TouchEvent
        {
            public long Id { get; }
            public Geometries.Point Location { get; }
            public long Tick { get; }

            public TouchEvent(long id, Geometries.Point location, long tick)
            {
                Id = id;
                Location = location;
                Tick = tick;
            }
        }

        private const int None = 0;
        private const int Dragging = 1;
        private const int Zoom = 2;
        private const int shortTap = 125;
        private const int longTap = 500;

        private Map _map;
        private readonly MapRenderer _renderer = new MapRenderer();
        private float _skiaScale;
        private int _mode = None;
        private double _innerRotation;
        private Geometries.Point _previousCenter = new Geometries.Point();
        private double _previousAngle;
        private double _previousRadius = 1f;
        private Dictionary<long, TouchEvent> _touches = new Dictionary<long, TouchEvent>();
        private Timer _doubleTapTestTimer;
        private int _numOfTaps = 0;

        public event EventHandler ViewportInitialized;
        public event EventHandler<TouchEventArgs> TouchStarted;
        public event EventHandler<TouchEventArgs> TouchEnded;
        public event EventHandler<TouchEventArgs> TouchMoved;
        public event EventHandler<HoverEventArgs> Hovered;
        public event EventHandler<TapEventArgs> SingleTapped;
        public event EventHandler<TapEventArgs> LongTapped;
        public event EventHandler<TapEventArgs> DoubleTapped;
        public event EventHandler<ZoomEventArgs> Zoomed;

        public MapControl()
        {
            Initialize();
        }

        public void Initialize()
        {
            Map = new Map();
            BackgroundColor = Color.White;

            TryInitializeViewport();

            EnableTouchEvents = true;

            PaintSurface += OnPaintSurface;
            Touch += OnTouch;
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            if (_map.Viewport.TryInitializeViewport(_map, CanvasSize.Width, CanvasSize.Height))
            {
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            // Save time, when the event occures
            long ticks = DateTime.Now.Ticks;

            var location = new Geometries.Point((e.Location.X - Bounds.Left) / _skiaScale, (e.Location.Y - Bounds.Top) / _skiaScale);

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _touches[e.Id] = new TouchEvent(e.Id, location, ticks);

                // Do we have a doubleTapTestTimer running?
                // If yes, stop it and increment _numOfTaps
                if (_doubleTapTestTimer != null)
                {
                    _doubleTapTestTimer.Cancel();
                    _doubleTapTestTimer = null;
                    _numOfTaps++;
                }
                OnTouchStarted(_touches.Select(t => t.Value.Location).ToList());
            }
            if (e.ActionType == SKTouchAction.Released)
            {
                // Do we have a tap event
                if (_touches[e.Id].Location.Equals(location) && ticks - _touches[e.Id].Tick < shortTap * 10000)
                {
                    // Start a timer with timeout 180 ms. If than isn't arrived another tap, than it is a single
                    _doubleTapTestTimer = new Timer((l) =>
                    {
                        if (_numOfTaps > 1)
                        {
                            OnDoubleTapped(location, _numOfTaps);
                        }
                        else
                            OnSingleTapped((Geometries.Point)l);
                        _numOfTaps = 1;
                        _doubleTapTestTimer = null;
                    }, location, 180, Timeout.Infinite);
                }
                else if (_touches[e.Id].Location.Equals(location) && ticks - _touches[e.Id].Tick < longTap * 10000)
                {
                    OnLongTapped(location);
                }
                var releasedTouch = _touches[e.Id];
                _touches.Remove(e.Id);

                OnTouchEnded(_touches.Select(t => t.Value.Location).ToList(), new Geometries.Point((releasedTouch.Location.X - Bounds.Left) / _skiaScale, (releasedTouch.Location.Y - Bounds.Top) / _skiaScale));
            }
            if (e.ActionType == SKTouchAction.Moved)
            {
                _touches[e.Id] = new TouchEvent(e.Id, location, DateTime.Now.Ticks);

                if (e.InContact)
                    OnTouchMoved(_touches.Select(t => t.Value.Location).ToList());
                else
                    OnHover(_touches.Select(t => t.Value.Location).FirstOrDefault());
            }

            e.Handled = true;
        }

        private bool OnZoomedOut(Geometries.Point location)
        {
            var handler = Zoomed;
            var eventArgs = new ZoomEventArgs(location, ZoomDirection.ZoomOut, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool OnZoomedIn(Geometries.Point location)
        {
            var handler = Zoomed;
            var eventArgs = new ZoomEventArgs(location, ZoomDirection.ZoomIn, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            // TODO
            // Perform standard behavior

            return true;
        }

        private bool OnHover(Geometries.Point location)
        {
            var handler = Hovered;
            var eventArgs = new HoverEventArgs(location, false);

            handler?.Invoke(this, eventArgs);

            return eventArgs.Handled;
        }

        private bool OnTouchStarted(List<Geometries.Point> touchPoints)
        {
            var handler = TouchStarted;
            var eventArgs = new TouchEventArgs(touchPoints, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            if (touchPoints.Count >= 2)
            {
                (_previousCenter, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                _mode = Zoom;
                _innerRotation = _map.Viewport.Rotation;
            }
            else
            {
                _mode = Dragging;
                _previousCenter = touchPoints.First();
            }

            return true;
        }

        private bool OnTouchEnded(List<Geometries.Point> touchPoints, Geometries.Point releasedPoint)
        {
            var handler = TouchEnded;
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

        private bool OnTouchMoved(List<Geometries.Point> touchPoints)
        {
            var handler = TouchMoved;
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
                case Zoom:
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

        private bool OnDoubleTapped(Geometries.Point location, int numOfTaps)
        {
            var handler = DoubleTapped;
            var eventArgs = new TapEventArgs(location, numOfTaps, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            var tapWasHandled = Map.InvokeInfo(location, location, _skiaScale, _renderer.SymbolCache, WidgetTouched, numOfTaps);

            if (!tapWasHandled)
            {
                // Double tap as zoom
                return OnZoomedIn(location);
            }

            return false;
        }

        private bool OnSingleTapped(Geometries.Point location)
        {
            var handler = SingleTapped;
            var eventArgs = new TapEventArgs(location, 1, false);

            handler?.Invoke(this, eventArgs);

            if (eventArgs.Handled)
                return true;

            return Map.InvokeInfo(location, location, _skiaScale, _renderer.SymbolCache, WidgetTouched, 1);
        }

        private bool OnLongTapped(Geometries.Point location)
        {
            var handler = LongTapped;
            var eventArgs = new TapEventArgs(location, 1, false);

            handler?.Invoke(this, eventArgs);

            return eventArgs.Handled;
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            _map.Viewport.Width = Width;
            _map.Viewport.Height = Height;

            _skiaScale = (float)(CanvasSize.Width / Width);
            skPaintSurfaceEventArgs.Surface.Canvas.Scale(_skiaScale, _skiaScale);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas,
                _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

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
        
        private Geometries.Point GetScreenPosition(SKPoint point)
        {
            return new Geometries.Point(point.X * _skiaScale, point.Y * _skiaScale);
        }

        public void Refresh()
        {
            RefreshGraphics();
            RefreshData();
        }

        public Map Map
        {
            get => _map;
            set
            {
                if (_map != null)
                {
                    UnsubscribeFromMapEvents(_map);
                    _map = null;
                }

                _map = value;

                if (_map != null)
                {
                    SubscribeToMapEvents(_map);
                    _map.ViewChanged(true);
                }

                RefreshGraphics();
            }
        }

        private void MapRefreshGraphics(object sender, EventArgs eventArgs)
        {
            RefreshGraphics();
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layers.Layer.Enabled))
            {
                RefreshGraphics();
            }
            else if (e.PropertyName == nameof(Layers.Layer.Opacity))
            {
                RefreshGraphics();
            }
        }

        private void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            string errorMessage;

            Device.BeginInvokeOnMainThread(delegate
            {
                try
                {
                    if (e == null)
                    {
                        errorMessage = "MapDataChanged Unexpected error: DataChangedEventArgs can not be null";
                        System.Diagnostics.Debug.WriteLine(errorMessage);
                    }
                    else if (e.Cancelled)
                    {
                        errorMessage = "MapDataChanged: Cancelled";
                        System.Diagnostics.Debug.WriteLine(errorMessage);
                    }
                    else if (e.Error is System.Net.WebException)
                    {
                        errorMessage = "MapDataChanged WebException: " + e.Error.Message;
                        System.Diagnostics.Debug.WriteLine(errorMessage);
                    }
                    else if (e.Error != null)
                    {
                        errorMessage = "MapDataChanged errorMessage: " + e.Error.GetType() + ": " + e.Error.Message;
                        System.Diagnostics.Debug.WriteLine(errorMessage);
                    }

                    RefreshGraphics();
                }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Warning, "Unexpected exception in MapDataChanged", exception);
                }
            });
        }

        public void RefreshGraphics()
        {
            InvalidateSurface();
        }

        public void RefreshData()
        {
            _map?.ViewChanged(true);
        }

        public bool AllowPinchRotation { get; set; }
        public double UnSnapRotationDegrees { get; set; }
        public double ReSnapRotationDegrees { get; set; }

        public Geometries.Point WorldToScreen(Geometries.Point worldPosition)
        {
            return SharedMapControl.WorldToScreen(Map.Viewport, _skiaScale, worldPosition);
        }

        public Geometries.Point ScreenToWorld(Geometries.Point screenPosition)
        {
            return SharedMapControl.ScreenToWorld(Map.Viewport, _skiaScale, screenPosition);
        }

        private static void WidgetTouched(IWidget widget, Geometries.Point screenPosition)
        {
            widget.HandleWidgetTouched(screenPosition);
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Unsubscribe()
        {
            UnsubscribeFromMapEvents(_map);
        }

        private void SubscribeToMapEvents(Map map)
        {
            map.DataChanged += MapDataChanged;
            map.PropertyChanged += MapPropertyChanged;
            map.RefreshGraphics += MapRefreshGraphics;
        }

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