using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mapsui.Fetcher;
using Mapsui.Layers;
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
        private PointF _previousMap, _currentMap;
        private PointF _previousMid = new PointF();
        private readonly PointF _currentMid = new PointF();
        private float _oldDist = 1f;
        private bool _viewportInitialized;
        private Rendering.Skia.MapRenderer _renderer;
        private SKCanvasView _canvas;
        private Map _map;
        private AttributionPanel _attributionPanel;
        private float _scale;
        
        public event EventHandler ViewportInitialized;

        public MapControl(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public MapControl(Context context, IAttributeSet attrs, int defStyle):
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public void Initialize()
        {
            _scale = Resources.DisplayMetrics.Density;

            _canvas = new SKCanvasView(Context);
            _canvas.PaintSurface += CanvasOnPaintSurface;
            AddView(_canvas);

            AddView(_attributionPanel = new AttributionPanel(Context));
            
            Map = new Map();
            _renderer = new Rendering.Skia.MapRenderer();
            InitializeViewport();
            Touch += MapView_Touch;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            Map.Viewport.Width = Width / _scale;
            Map.Viewport.Height = Height / _scale;

            base.OnSizeChanged(w, h, oldw, oldh);
        }

        private void CanvasOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (!_viewportInitialized)
                InitializeViewport();
            if (!_viewportInitialized)
                return;

            args.Surface.Canvas.Scale(_scale, _scale);

            _renderer.Render(args.Surface.Canvas, _map.Viewport, _map.Layers, _map.BackColor);
        }

        private void InitializeViewport()
        {
            if (ViewportHelper.TryInitializeViewport(_map, Width / _scale, Height / _scale))
            {
                _viewportInitialized = true;
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
            var x = (int)args.Event.RawX;
            var y = (int)args.Event.RawY;
            switch (args.Event.Action)
            {
                case MotionEventActions.Down:
                    _previousMap = null;
                    _mode = Dragging;
                    break;
                case MotionEventActions.Up:
                    _previousMap = null;
                    Invalidate();
                    _mode = None;
                    _map.ViewChanged(true);
                    Map.InvokeInfo(GetPosition(args.Event).ToMapsui());
                    break;
                case MotionEventActions.Pointer2Down:
                    _previousMap = null;
                    _oldDist = Spacing(args.Event);
                    MidPoint(_currentMid, args.Event);
                    _previousMid = _currentMid;
                    _mode = Zoom;
                    break;
                case MotionEventActions.Pointer2Up:
                    _previousMap = null;
                    _previousMid = null;
                    _mode = Dragging;
                    break;
                case MotionEventActions.Move:
                    switch (_mode)
                    {
                        case Dragging:
                            _currentMap = new PointF(x, y);
                            if (_previousMap != null)
                            {
                                _map.Viewport.Transform(
                                    _currentMap.X  / _scale,
                                    _currentMap.Y / _scale,
                                    _previousMap.X / _scale,
                                    _previousMap.Y / _scale);
                                Invalidate();
                            }
                            _previousMap = _currentMap;
                            break;
                        case Zoom:
                            {
                                if (args.Event.PointerCount < 2)
                                    return;

                                var newDist = Spacing(args.Event);
                                var scale = newDist / _oldDist;

                                _oldDist = Spacing(args.Event);
                                _previousMid = new PointF(_currentMid.X, _currentMid.Y);
                                MidPoint(_currentMid, args.Event);

                                _map.Viewport.Transform(
                                    _currentMid.X / _scale,
                                    _currentMid.Y / _scale,
                                    _previousMid.X / _scale,
                                    _previousMid.Y / _scale,
                                    scale);
                                Invalidate();
                            }
                            break;
                    }
                    break;
            }
        }

        private static float Spacing(MotionEvent me)
        {
            if (me.PointerCount < 2)
                throw new ArgumentException();

            var x = me.GetX(0) - me.GetX(1);
            var y = me.GetY(0) - me.GetY(1);
            return (float)Math.Sqrt(x * x + y * y);
        }

        private static void MidPoint(PointF point, MotionEvent motionEvent)
        {
            var position = GetPosition2(motionEvent);
            point.Set(position.X / 2, position.Y / 2);
        }
        
        private static PointF GetPosition2(MotionEvent motionEvent)
        {
            return new PointF(motionEvent.GetX(0) + motionEvent.GetX(1), motionEvent.GetY(0) + motionEvent.GetY(1));
        }

        private static PointF GetPosition(MotionEvent motionEvent)
        {
            return new PointF(motionEvent.GetX(0) , motionEvent.GetY(0));
        }

        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                if (_map != null)
                {
                    var temp = _map;
                    _map = null;
                    temp.DataChanged -= MapDataChanged;
                    temp.PropertyChanged -= MapPropertyChanged;
                    temp.RefreshGraphics -= MapRefreshGraphics;
                    temp.Dispose();
                    _attributionPanel.Clear();
                }

                _map = value;

                if (_map != null)
                {
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.RefreshGraphics += MapRefreshGraphics;
                    _map.ViewChanged(true);
                    _attributionPanel.Populate(Map.Layers);
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
            else if (e.PropertyName == nameof(Map.Layers))
            {
                _attributionPanel.Populate(Map.Layers);
            }
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                // todo: test code below:
                ((Activity)Context).RunOnUiThread(new Runnable(Toast.MakeText(Context, e.Error.Message, ToastLength.Short).Show));
            }
            else // no problems
            {
                ((Activity)Context).RunOnUiThread(new Runnable(RefreshGraphics));
            }
        }

        public void RefreshGraphics()
        {
            PostInvalidate();
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            Position(_canvas, l, t, r, b);
            UpdateSize(_attributionPanel);
            PositionBottomRight(_attributionPanel);
        }

        private void Position(View view, int l, int t, int r, int b)
        {
            view.Top = t;
            view.Bottom = b;
            view.Left = l;
            view.Right = r;
        }

        private void PositionBottomRight(View view)
        {
            Position(view, Right - view.Width, Bottom - view.Height, Right, Bottom);
        }

        private static void UpdateSize(View view)
        {
            // I created this method because I don't understand what I'm doing
            view.Measure(0, 0);
            view.Right = view.Left + view.MeasuredWidth;
            view.Bottom = view.Top + view.MeasuredHeight;
        }
    }
}