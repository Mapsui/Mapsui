using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Mapsui.Fetcher;
using Mapsui.Rendering.OpenTK;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Android;
using System;
using System.ComponentModel;
using Math = System.Math;
using Android.App;
using Java.Lang;

namespace Mapsui.UI.Android
{
    public class MapControl : AndroidGameView
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
        private MapRenderer _renderer;
        private Map _map;
        private bool _refreshGraphics;
        public event  EventHandler<EventArgs> ViewportInitialized;
        
        public MapControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public MapControl(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            Map = new Map();
            _renderer = new MapRenderer();
            InitializeViewport();
            Touch += MapControl_Touch;
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Disable(All.DepthTest);
            Run(60); 
        }

        private void InitializeViewport()
        {
            if (_viewportInitialized) return;
            if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
            if (_map == null) return;
            if (_map.Envelope == null) return;
            if (Math.Abs(_map.Envelope.Width - 0d) < Utilities.Constants.Epsilon) return;
            if (Math.Abs(_map.Envelope.Height - 0d) < Utilities.Constants.Epsilon) return;
            if (_map.Envelope.GetCentroid() == null) return;

            if (double.IsNaN(_map.Viewport.Resolution))
                _map.Viewport.Resolution = _map.Envelope.Width / Width;
            if (double.IsNaN(_map.Viewport.Center.X) || double.IsNaN(_map.Viewport.Center.Y))
                _map.Viewport.Center = _map.Envelope.GetCentroid();
            _map.Viewport.Width = Width;
            _map.Viewport.Height = Height;
            if (Width >= 1080 && Height >= 1080) _map.Viewport.RenderResolutionMultiplier = 2;

            _viewportInitialized = true;
            OnViewportInitialized();
            _map.ViewChanged(true);
        }

        private void ViewportOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Do nothing. For performance it is better to have more specific control
        }

        void MapControl_Touch(object sender, TouchEventArgs args)
        {
            if (_map.Lock) return;

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
                _mode = None;
                _map.ViewChanged (true);
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
                _map.ViewChanged (true);
                break;
            case MotionEventActions.Move:
                switch (_mode)
                {
                case Drag:
                    _currentMap = new PointF (x, y);
                    if (_previousMap != null) {
                        _map.Viewport.Transform (_currentMap.X, _currentMap.Y, _previousMap.X, _previousMap.Y);
                        RefreshGraphics ();
                    }
                    _previousMap = _currentMap;                    
                    break;
                case Zoom:
                    if (args.Event.PointerCount < 2) return;

                    var newDist = Spacing (args.Event);
                    var scale = newDist / _oldDist;

                    _oldDist = Spacing (args.Event);
                    _previousMid = new PointF (_currentMid.X, _currentMid.Y);
                    MidPoint (_currentMid, args.Event);
                    _map.Viewport.Transform(_currentMid.X, _currentMid.Y, _previousMid.X, _previousMid.Y, scale);
                    RefreshGraphics ();
                    break;
                }
                break;
            }
        }

        private static float Spacing(MotionEvent me)
        {
            if (me.PointerCount < 2) throw new ArgumentException();

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
                    _map.ViewChanged(true);
                }
                RefreshGraphics();
            }
        }

        private void RefreshGraphics()
        {
            _refreshGraphics = true;
            Invalidate ();
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Envelope")
            {
                InitializeViewport();
                _map.ViewChanged(true);
                RefreshGraphics();
            }
            else if (e.PropertyName == "Enabled")
            {
                RefreshGraphics();
            }
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                //todo test code below:
                //((Activity)Context).RunOnUiThread(new Runnable(Toast.MakeText(Context, GetErrorMessage(e), ToastLength.Short).Show));
            }
            else // no problems
            {
                ((Activity)Context).RunOnUiThread(new Runnable(() =>RefreshGraphics()));
            }
        }

        private static string GetErrorMessage(DataChangedEventArgs e)
        {
            return (e.Cancelled) ? "Cancelled" : ((e.Error != null) ? e.Error.GetType() + ": " + e.Error.Message : "");
        }

        private void Set2DViewport()
        {
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 1);
            GL.MatrixMode(All.Modelview);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (!_refreshGraphics) return;
            _refreshGraphics = false;

            if (!_viewportInitialized)
                InitializeViewport();
            if (!_viewportInitialized)
                return;

            Set2DViewport();

            GL.ClearColor(_map.BackColor.R, _map.BackColor.G, _map.BackColor.B, _map.BackColor.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);

           _renderer.Render(_map.Viewport, _map.Layers);

            SwapBuffers();
        }

        private void OnViewportInitialized()
        {
            var handler = ViewportInitialized;
            if (handler != null) handler(this, new EventArgs());
        }
    }
}