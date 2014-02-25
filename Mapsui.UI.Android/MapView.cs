using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mapsui.Fetcher;
using Mapsui.Rendering.Android;
using System;
using System.ComponentModel;
using Math = System.Math;

namespace Mapsui.UI.Android
{
    public class MapView : View//, View.IOnTouchListener
    {
        private const int None = 0;
        private const int Drag = 1;
        private const int Zoom = 2;
        private int _mode = None;
        private PointF _previousMap, _currentMap;
        private PointF _previousMid = new PointF();
        private readonly PointF _currentMid = new PointF();
        private float _oldDist = 1f;
        private bool _viewportInitialized;

        public MapView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public MapView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private MapRenderer _renderer;
        private Map _map;

        private void ViewportOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            RefreshGraphics();
        }

        public void Initialize()
        {
            Map = new Map();
            _renderer = new MapRenderer();
            InitializeViewport();
            Touch += MapView_Touch;
        }

        private void InitializeViewport()
        {
            if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
            if (_map == null) return;
            if (_map.Envelope == null) return;
            if (Math.Abs(_map.Envelope.Width - 0d) < Utilities.Constants.Epsilon) return;
            if (Math.Abs(_map.Envelope.Height - 0d) < Utilities.Constants.Epsilon) return;
            if (_map.Envelope.GetCentroid() == null) return;

            if (double.IsNaN(_map.Viewport.Resolution))
                _map.Viewport.Resolution = _map.Envelope.Width / Width;
            if (double.IsNaN(_map.Viewport.CenterX) || double.IsNaN(_map.Viewport.CenterY))
                _map.Viewport.Center = _map.Envelope.GetCentroid();
            _map.Viewport.Width = Width;
            _map.Viewport.Height = Height;
            _map.Viewport.RenderScaleFactor = 2;

            _map.ViewChanged(true, _map.Viewport.Extent, _map.Viewport.RenderResolution);
            _viewportInitialized = true;
        }

        void MapView_Touch(object sender, TouchEventArgs args)
        {

            var x = (int)args.Event.RawX;
            var y = (int)args.Event.RawY;
            switch (args.Event.Action)
            {
                case MotionEventActions.Down:
                    _previousMap = null;
                    _mode = Drag;
                    break;
                case MotionEventActions.Up:
                    _previousMap = null;
                    Invalidate();
                    _mode = None;
                    _map.ViewChanged(false, _map.Viewport.Extent, _map.Viewport.Resolution);
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
                    _mode = Drag;
                    break;
                case MotionEventActions.Move:
                    switch (_mode)
                    {
                        case Drag:
                            _currentMap = new PointF(x, y);
                            if (_previousMap != null)
                            {
                                _map.Viewport.Transform(
                                    _currentMap.X,
                                    _currentMap.Y,
                                    _previousMap.X,
                                    _previousMap.Y);
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
                                _map.Viewport.Center = _map.Viewport.ScreenToWorld(
                                    _currentMid.X,
                                    _currentMid.Y);
                                _map.Viewport.Resolution = _map.Viewport.Resolution / scale;
                                _map.Viewport.Center = _map.Viewport.ScreenToWorld(
                                    (_map.Viewport.Width - _currentMid.X),
                                    (_map.Viewport.Height - _currentMid.Y));
                                _map.Viewport.Transform(
                                    _currentMid.X,
                                    _currentMid.Y,
                                    _previousMid.X,
                                    _previousMid.Y);
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

        private static void MidPoint(PointF point, MotionEvent me)
        {
            var x = me.GetX(0) + me.GetX(1);
            var y = me.GetY(0) + me.GetY(1);
            point.Set(x / 2, y / 2);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected void UnloadContent()
        {
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
                    temp.PropertyChanged -= MapPropertyChanged;
                    temp.Dispose();
                }

                _map = value;
                //all changes of all layers are returned through this event handler on the map
                if (_map != null)
                {
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.Viewport.PropertyChanged += ViewportOnPropertyChanged; // not sure if this should be a direct coupling 
                    _map.ViewChanged(true, _map.Viewport.Extent, _map.Viewport.RenderResolution);
                }
                RefreshGraphics();
            }
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Envelope") return;
            InitializeViewport();
            _map.ViewChanged(true, _map.Viewport.Extent, _map.Viewport.RenderResolution);
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                Toast.MakeText(Context, GetErrorMessage(e), ToastLength.Short).Show();
            }
            else // no problems
            {
                ((Activity)Context).RunOnUiThread(new Runnable(RefreshGraphics));
            }
        }

        private static string GetErrorMessage(DataChangedEventArgs e)
        {
            return (e.Cancelled) ? "Cancelled" : ((e.Error != null) ? e.Error.GetType() + ": " + e.Error.Message : "");
        }

        private void RefreshGraphics() //should be private soon
        {
            PostInvalidate();
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (!_viewportInitialized)
                InitializeViewport();
            if (!_viewportInitialized)
                return;

            _renderer.Canvas = canvas;

            _renderer.Render(_map.Viewport, _map.Layers);
        }
    }
}