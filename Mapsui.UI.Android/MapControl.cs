using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Java.Lang;
using Mapsui.Fetcher;
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
        private PointF _touchPosition;
        private PointF _previousTouchPosition;
        private PointF _touchCenter = new PointF();
        private PointF _previousTouchCenter = new PointF();
        private PointF _touchDownPosition = new PointF();
        private double _previousAngle;
        private float _previousDistance = 1f;
        private Rendering.Skia.MapRenderer _renderer;
        private SKCanvasView _canvas;
        private Map _map;
        private float _scale;
        private double _innerRotation;

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
            var x = (int)args.Event.GetX(0);
            var y = (int)args.Event.GetY(0);

            switch (args.Event.Action)
            {
                case MotionEventActions.Down:
                    _previousTouchPosition = null;
                    _touchDownPosition = new PointF(x, y);
                    _mode = Dragging;
                    break;
                case MotionEventActions.Up:
                    _canvas.Invalidate();
                    _mode = None;
                    _map.ViewChanged(true);
                    var position = GetScreenPosition(args.Event);
                    Map.InvokeInfo(position, _touchDownPosition.ToMapsui(), _scale, _renderer.SymbolCache, WidgetTouch);
                    break;
                case MotionEventActions.Pointer2Down:
                    _previousTouchPosition = null;
                    _previousDistance = DistanceBetweenTouches(args.Event);
                    _touchCenter = GetTouchCenter(args.Event);
                    _previousTouchCenter = _touchCenter;
                    if (AllowPinchRotation)
                    {
                        _previousAngle = Angle(args.Event);
                        _innerRotation = _map.Viewport.Rotation;
                    }
                    _touchDownPosition = _touchCenter;
                    _mode = Zoom;
                    break;
                case MotionEventActions.Pointer2Up:
                    _previousTouchPosition = null;
                    _previousTouchCenter = null;
                    _mode = Dragging;
                    break;
                case MotionEventActions.Move:
                    switch (_mode)
                    {
                        case Dragging:
                            _touchPosition = new PointF(x, y);
                            if (_previousTouchPosition != null)
                            {
                                _map.Viewport.Transform(
                                    _touchPosition.X / _scale,
                                    _touchPosition.Y / _scale,
                                    _previousTouchPosition.X / _scale,
                                    _previousTouchPosition.Y / _scale);

                                ViewportLimiter.LimitExtent(_map.Viewport,
                                    _map.PanMode, _map.PanLimits, _map.Envelope);

                                _canvas.Invalidate();
                            }
                            _previousTouchPosition = _touchPosition;
                            break;
                        case Zoom:
                            {
                                if (args.Event.PointerCount < 2) return;

                                var distance = DistanceBetweenTouches(args.Event);
                                var scale = distance / _previousDistance;
                                _previousDistance = distance;

                                _previousTouchCenter = new PointF(_touchCenter.X, _touchCenter.Y);
                                _touchCenter = GetTouchCenter(args.Event);

                                _map.Viewport.Transform(
                                    _touchCenter.X / _scale,
                                    _touchCenter.Y / _scale,
                                    _previousTouchCenter.X / _scale,
                                    _previousTouchCenter.Y / _scale,
                                    scale);

                                if (AllowPinchRotation)
                                {
                                    var angle = Angle(args.Event);
                                    _innerRotation += angle - _previousAngle;
                                    _innerRotation %= 360;

                                    if (_innerRotation > 180)
                                        _innerRotation -= 360;
                                    else if (_innerRotation < -180)
                                        _innerRotation += 360;

                                    if (_map.Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                                        _map.Viewport.Rotation = _innerRotation;
                                    else if (_map.Viewport.Rotation != 0)
                                    {
                                        if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                                            _map.Viewport.Rotation = 0;
                                        else
                                            _map.Viewport.Rotation = _innerRotation;
                                    }

                                    _previousAngle = angle;
                                }

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

        private static double Angle(MotionEvent me)
        {
            if (me.PointerCount < 2)
                throw new ArgumentException();
            var x = me.GetX(1) - me.GetX(0);
            var y = me.GetY(1) - me.GetY(0);
            var rotation = Math.Atan2(y, x) * 180.0 / Math.PI;
            return rotation;
        }

        private static float DistanceBetweenTouches(MotionEvent me)
        {
            if (me.PointerCount < 2)
                throw new ArgumentException();

            var x = me.GetX(0) - me.GetX(1);
            var y = me.GetY(0) - me.GetY(1);
            return (float)Math.Sqrt(x * x + y * y);
        }

        private static PointF GetTouchCenter(MotionEvent motionEvent)
        {
            return new PointF(
                (motionEvent.GetX(0) + motionEvent.GetX(1)) / 2,
                (motionEvent.GetY(0) + motionEvent.GetY(1)) / 2);
        }

        private static Geometries.Point GetScreenPosition(MotionEvent motionEvent)
        {
            return new PointF(motionEvent.GetX(0), motionEvent.GetY(0)).ToMapsui();
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

        private void WidgetTouch(IWidget widget)
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