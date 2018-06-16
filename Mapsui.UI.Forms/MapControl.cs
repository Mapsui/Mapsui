using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using Mapsui.Logging;
using Mapsui.Widgets;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SkiaSharp;
using Mapsui.Rendering;
using Mapsui.UI.Utils;

namespace Mapsui.UI.Forms
{
    public partial class MapControl : SKGLView, IMapControl, IDisposable
    {
        class TouchEvent
        {
            public long Id { get; }
            public Geometries.Point Location { get; }
            public long Tick { get; }

            public TouchEvent(long id, Geometries.Point screenPosition, long tick)
            {
                Id = id;
                Location = screenPosition;
                Tick = tick;
            }
        }

        private const int None = 0;
        private const int Dragging = 1;
        private const int Zooming = 2;
        // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
        private const int shortTap = 125;
        private const int shortClick = 250;
        private const int delayTap = 300;
        private const int longTap = 500;

        private Map _map;
        private readonly MapRenderer _renderer = new MapRenderer();
        private float _skiaScale;
        private double _innerRotation;
        private Dictionary<long, TouchEvent> _touches = new Dictionary<long, TouchEvent>();
        private Geometries.Point _firstTouch;
        private System.Threading.Timer _doubleTapTestTimer;
        private int _numOfTaps = 0;
        private Dictionary<long, int> _fingers = new Dictionary<long, int>(20);
        private VelocityTracker _velocityTracker = new VelocityTracker();

        public event EventHandler ViewportInitialized;

        public MapControl()
        {
            Initialize();
        }

        public float SkiaScale
        {
            get
            {
                return _skiaScale;
            }
        }

        public ISymbolCache SymbolCache
        {
            get
            {
                return _renderer.SymbolCache;
            }
        }

        public void Initialize()
        {
            Map = new Map();
            BackgroundColor = Color.White;

            TryInitializeViewport();

            EnableTouchEvents = true;

            PaintSurface += OnPaintSurface;
            Touch += OnTouch;
            SizeChanged += OnSizeChanged; 
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            _skiaScale = (float)(CanvasSize.Width / Width);

            if (_map.Viewport.TryInitializeViewport(_map, CanvasSize.Width / _skiaScale, CanvasSize.Height / _skiaScale))
            {
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (Map != null)
            {
                Map.Viewport.Width = Width;
                Map.Viewport.Height = Height;
            }
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            // Save time, when the event occures
            long ticks = DateTime.Now.Ticks;

            var location = GetScreenPosition(e.Location);

            // Get finger/handler for this event
            if (!_fingers.Keys.Contains(e.Id))
                if (_fingers.Count < 10)
                {
                    _fingers.Add(e.Id, _fingers.Count);
                }

            var id = _fingers[e.Id];

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _firstTouch = location;

                _touches[id] = new TouchEvent(id, location, ticks);

                _velocityTracker.Clear();

                // Do we have a doubleTapTestTimer running?
                // If yes, stop it and increment _numOfTaps
                if (_doubleTapTestTimer != null)
                {
                    _doubleTapTestTimer.Dispose();
                    _doubleTapTestTimer = null;
                    _numOfTaps++;
                }
                else
                    _numOfTaps = 1;

                e.Handled = OnTouchStart(_touches.Select(t => t.Value.Location).ToList());
            }
            if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _fingers, because finger is released
                
                _fingers.Remove(e.Id);

                double velocityX;
                double velocityY;

                (velocityX, velocityY) = _velocityTracker.CalcVelocity(id, ticks);

                // Is this a fling or swipe?
                if (velocityX > 10000 || velocityY > 10000)
                {
                    System.Diagnostics.Debug.WriteLine($"Velocity X = {velocityX}, Velocity Y = {velocityY}");

                    e.Handled = OnFlinged(velocityX, velocityY);
                }

                // Do we have a tap event
                var temp = _touches[id];
                if (_touches[id].Location.Equals(_firstTouch) && ticks - _touches[id].Tick < (e.DeviceType == SKTouchDeviceType.Mouse ? shortClick : longTap) * 10000)
                {
                    // Start a timer with timeout delayTap ms. If than isn't arrived another tap, than it is a single
                    _doubleTapTestTimer = new System.Threading.Timer((l) =>
                    {
                        if (_numOfTaps > 1)
                        {
                            if (!e.Handled)
                                e.Handled = OnDoubleTapped(location, _numOfTaps);
                        }
                        else
                            if (!e.Handled)
                                e.Handled = OnSingleTapped((Geometries.Point)l);
                        _numOfTaps = 1;
                        _doubleTapTestTimer.Dispose();
                        _doubleTapTestTimer = null;
                    }, location, delayTap, -1);
                }
                else if (_touches[id].Location.Equals(_firstTouch) && ticks - _touches[id].Tick >= longTap * 10000)
                {
                    if (!e.Handled)
                        e.Handled = OnLongTapped(location);
                }
                var releasedTouch = _touches[id];
                _touches.Remove(id);

                if (!e.Handled)
                    e.Handled = OnTouchEnd(_touches.Select(t => t.Value.Location).ToList(), releasedTouch.Location);
            }
            if (e.ActionType == SKTouchAction.Moved)
            {
                _touches[id] = new TouchEvent(id, location, ticks);

                if (e.InContact)
                    _velocityTracker.AddEvent(id, location, ticks);

                if (e.InContact && !e.Handled)
                    e.Handled = OnTouchMove(_touches.Select(t => t.Value.Location).ToList());
                else
                    e.Handled = OnHovered(_touches.Select(t => t.Value.Location).FirstOrDefault());
            }
        }

        void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            _map.Viewport.Width = Width;
            _map.Viewport.Height = Height;

            skPaintSurfaceEventArgs.Surface.Canvas.Scale(_skiaScale, _skiaScale);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas,
                _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        private Geometries.Point GetScreenPosition(SKPoint point)
        {
            return new Geometries.Point(point.X / _skiaScale, point.Y / _skiaScale);
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
            InvalidateCanvas();
        }

        public void RefreshData()
        {
            _map?.ViewChanged(true);
        }

        internal void InvalidateCanvas()
        {
            InvalidateSurface();
        }

        private static void WidgetTouched(IWidget widget, Geometries.Point screenPosition)
        {
            widget.HandleWidgetTouched(screenPosition);
        }
    }
}