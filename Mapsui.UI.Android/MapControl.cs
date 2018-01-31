using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Java.Lang;
using Mapsui.Fetcher;
using Mapsui.Geometries.Utilities;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Widgets;
using SkiaSharp.Views.Android;
using Math = System.Math;

namespace Mapsui.UI.Android
{
    public class MapControl : ViewGroup, IMapControl
    {
        private const int None = 0;
        private const int Dragging = 1;
        private const int Zoom = 2;
        private int _mode = None;
        private Geometries.Point _previousCenter = new Geometries.Point();
        private double _previousAngle;
        private double _previousRadius = 1f;
        private Rendering.Skia.MapRenderer _renderer;
        private SKCanvasView _canvas;
        private Map _map;
        private float _scale;
        private double _innerRotation;
        private GestureDetector _gestureDetector;

        public event EventHandler ViewportInitialized;

        public MapControl(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public MapControl(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public void Initialize()
        {
            SetBackgroundColor(Color.Transparent);
            _scale = Resources.DisplayMetrics.Density;

            _canvas = new SKCanvasView(Context);
            _canvas.PaintSurface += CanvasOnPaintSurface;
            AddView(_canvas);

            Map = new Map();
            _renderer = new Rendering.Skia.MapRenderer();
            TryInitializeViewport();
            Touch += MapView_Touch;
            
            _gestureDetector = new GestureDetector(Context, new GestureDetector.SimpleOnGestureListener());
            _gestureDetector.SingleTapConfirmed += TapGestureHandler;
        }
        
        private void TapGestureHandler(object sender, GestureDetector.SingleTapConfirmedEventArgs e)
        {
            var position = GetScreenPosition(e.Event, this);
            Map.InvokeInfo(position, position, _scale, _renderer.SymbolCache, WidgetTouched);
        }        

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            PushSizeOntoViewport();
        }

        void PushSizeOntoViewport()
        {
            if (Map != null)
            {
                Map.Viewport.Width = Width / _scale;
                Map.Viewport.Height = Height / _scale;
            }
        }

        private void CanvasOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            args.Surface.Canvas.Scale(_scale, _scale);

            _renderer.Render(args.Surface.Canvas, _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            if (_map.Viewport.TryInitializeViewport(_map, Width / _scale, Height / _scale))
            {
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void MapView_Touch(object sender, TouchEventArgs args)
        {
            if (_gestureDetector.OnTouchEvent(args.Event))
                return;

            var touchPoints = GetMapPositions(args.Event, this);

            switch (args.Event.Action)
            {
                case MotionEventActions.Up:
                    _canvas.Invalidate();
                    _mode = None;
                    _map.ViewChanged(true);
                    break;
                case MotionEventActions.Down:
                case MotionEventActions.Pointer1Down:
                case MotionEventActions.Pointer2Down:
                case MotionEventActions.Pointer3Down:
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
                    break;
                case MotionEventActions.Pointer1Up:
                case MotionEventActions.Pointer2Up:
                case MotionEventActions.Pointer3Up:
                    // Remove the touchPoint that was released from the locations to reset the
                    // starting points of the move and rotation
                    touchPoints.RemoveAt(args.Event.ActionIndex);           

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
                    break;
                case MotionEventActions.Move:
                    switch (_mode)
                    {
                        case Dragging:
                            {
                                if (touchPoints.Count != 1)
                                    return;

                                var touchPosition = touchPoints.First();
                                if (_previousCenter != null && !_previousCenter.IsEmpty())
                                {
                                    _map.Viewport.Transform(touchPosition.X, touchPosition.Y, _previousCenter.X, _previousCenter.Y);

                                    ViewportLimiter.LimitExtent(_map.Viewport, _map.PanMode, _map.PanLimits, _map.Envelope);

                                    _canvas.Invalidate();
                                }
                                _previousCenter = touchPosition;
                            }
                            break;
                        case Zoom:
                            {
                                if (touchPoints.Count < 2)
                                    return;

                                var (prevCenter, prevRadius, prevAngle) = (_previousCenter, _previousRadius, _previousAngle);
                                var (center, radius, angle ) = GetPinchValues(touchPoints);

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

                                _canvas.Invalidate();
                            }
                            break;
                    }
                    break;
            }
        }

        private List<Geometries.Point> GetMapPositions(MotionEvent me, View view)
        {
            var result = new List<Geometries.Point>();
            for (var i = 0; i < me.PointerCount; i++)
            {
                result.Add(new Geometries.Point((me.GetX(i) - view.Left) / _scale, (me.GetY(i) - view.Top) / _scale));
            }
            return result;
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

        private static Geometries.Point GetScreenPosition(MotionEvent motionEvent, View view)
        {
            return new PointF(
                motionEvent.GetX(0) - view.Left, 
                motionEvent.GetY(0) - view.Top).ToMapsui();
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
                    PushSizeOntoViewport();
                }

                RefreshGraphics();
            }
        }

        private void MapRefreshGraphics(object sender, EventArgs eventArgs)
        {
            ((Activity)Context).RunOnUiThread(new Runnable(RefreshGraphics));
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layer.Enabled))
            {
                RefreshGraphics();
            }
            else if (e.PropertyName == nameof(Layer.Opacity))
            {
                RefreshGraphics();
            }
        }

        private void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                Logger.Log(LogLevel.Warning, "An error occurred while fetching data", e.Error);
            }
            else if (e.Cancelled)
            {
                Logger.Log(LogLevel.Warning, "Fetching data was cancelled", e.Error);
            }
            else // no problems
            {
                RefreshGraphics();
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            Invalidate();
            base.OnDraw(canvas);
        }

        public void RefreshGraphics()
        {
            _canvas.PostInvalidate();
        }

        public void RefreshData()
        {
            _map.ViewChanged(true);
        }

        public void Refresh()
        {
            RefreshData();
            RefreshGraphics();
        }

        public bool AllowPinchRotation { get; set; }
        public double UnSnapRotationDegrees { get; set; } 
        public double ReSnapRotationDegrees { get; set; }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            Position(_canvas, l, t, r, b);
        }

        private void Position(View view, int l, int t, int r, int b)
        {
            view.Top = t;
            view.Bottom = b;
            view.Left = l;
            view.Right = r;
        }

        public Geometries.Point WorldToScreen(Geometries.Point worldPosition)
        {
            return SharedMapControl.WorldToScreen(Map.Viewport, _scale, worldPosition);
        }

        public Geometries.Point ScreenToWorld(Geometries.Point screenPosition)
        {
            return SharedMapControl.ScreenToWorld(Map.Viewport, _scale, screenPosition);
        }

        private void WidgetTouched(IWidget widget, Mapsui.Geometries.Point screenPosition)
        {
            if (widget is Hyperlink)
            {
                var hyperlink = (Hyperlink)widget;
                global::Android.Net.Uri uri = global::Android.Net.Uri.Parse(hyperlink.Url);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetData(uri);

                Intent chooser = Intent.CreateChooser(intent, "Open with");

                Context.StartActivity(chooser);
            }

            widget.HandleWidgetTouched(screenPosition);
        }

        protected override void Dispose(bool disposing)
        {
            Unsubscribe();
            base.Dispose(disposing);
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