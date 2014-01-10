using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Java.Lang;
using Mapsui.Fetcher;
using Mapsui.Rendering.Android;
using System;
using System.ComponentModel;
using Math = System.Math;

namespace Mapsui.UI.Android
{
    public class MapControl : View, View.IOnTouchListener
    {
        private const int None = 0;
	    private const int Drag = 1;
        private const int Zoom = 2;
        private int _mode = None;
        private PointF _previousMap, _currentMap;
        private PointF _previousMid = new PointF();
        private readonly PointF _currentMid = new PointF();
        private float _oldDist = 1f;
        private const float OutputMultiplier = 2;
        private const float InvertedOutputMultiplier = (1/OutputMultiplier);

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

        private MapRenderer _renderer;
        private Map _map;
        private readonly Viewport _viewport = new Viewport { CenterX = double.NaN, CenterY = double.NaN, Resolution = double.NaN };
        private bool _viewportInitialized;

        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public void Initialize()
        {
            Map = new Map();
            _renderer = new MapRenderer { OutputMultiplier = OutputMultiplier };
            InitializeViewport();
            SetOnTouchListener(this);
        }

        private void InitializeViewport()
        {
            if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
            if (_map == null) return;
            if (_map.Envelope == null) return;
            if (Math.Abs(_map.Envelope.Width - 0d) < Utilities.Constants.Epsilon) return;
            if (Math.Abs(_map.Envelope.Height - 0d) < Utilities.Constants.Epsilon) return;
            if (_map.Envelope.GetCentroid() == null) return;

            if (double.IsNaN(_viewport.Resolution))
                _viewport.Resolution = _map.Envelope.Width / Width;
            if (double.IsNaN(_viewport.CenterX) || double.IsNaN(_viewport.CenterY))
                _viewport.Center = _map.Envelope.GetCentroid();
            _viewport.Width = Width * InvertedOutputMultiplier;
            _viewport.Height = Height * InvertedOutputMultiplier;

            _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
            _viewportInitialized = true;
        }

        public bool OnTouch(View view, MotionEvent args)
        {            
            var x = (int)args.RawX;
            var y = (int)args.RawY;
            switch (args.Action)
            {
                case MotionEventActions.Down:
                    _previousMap = null;
                    _mode = Drag;
                    break;
                case MotionEventActions.Up:
                    _previousMap = null;
			        Invalidate();
                    _mode = None;
                    _map.ViewChanged(false, _viewport.Extent, _viewport.Resolution);
                    break;
                case MotionEventActions.Pointer2Down:
                    _previousMap = null;
                    _oldDist = Spacing(args);
		            MidPoint(_currentMid, args);
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
                              _viewport.Transform(
                                  _currentMap.X * InvertedOutputMultiplier, 
                                  _currentMap.Y * InvertedOutputMultiplier,
                                  _previousMap.X * InvertedOutputMultiplier,
                                  _previousMap.Y * InvertedOutputMultiplier);
                              Invalidate();
        	              }
        	              _previousMap = _currentMap;
        	              break;
        	          case Zoom:
        	              {
        	                  if (args.PointerCount < 2)
        	                      return true;

        	                  var newDist = Spacing(args);
                              var scale = newDist / _oldDist;

                              _oldDist = Spacing(args);	 
        	                  _previousMid = new PointF(_currentMid.X, _currentMid.Y);
        	                  MidPoint(_currentMid, args);
                              _viewport.Center = _viewport.ScreenToWorld(
                                  _currentMid.X * InvertedOutputMultiplier, 
                                  _currentMid.Y * InvertedOutputMultiplier);                        
        	                  _viewport.Resolution =  _viewport.Resolution / scale;
                              _viewport.Center = _viewport.ScreenToWorld(
                                  (_viewport.Width - _currentMid.X * InvertedOutputMultiplier), 
                                  (_viewport.Height - _currentMid.Y * InvertedOutputMultiplier));
                              _viewport.Transform(
                                  _currentMid.X * InvertedOutputMultiplier, 
                                  _currentMid.Y * InvertedOutputMultiplier, 
                                  _previousMid.X * InvertedOutputMultiplier, 
                                  _previousMid.Y * InvertedOutputMultiplier);
        	                  Invalidate();        
        	              }
        	              break;
        	      }
	              break;
            }
            return true;
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
                    _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
                }
                OnViewChanged();
                RefreshGraphics();
            }
        }

        public void OnViewChanged()
        {
            OnViewChanged(false);
        }

        private void OnViewChanged(bool userAction)
        {
            if (_map == null) return;
            if (ViewChanged != null)
            {
                ViewChanged(this, new ViewChangedEventArgs { Viewport = _viewport, UserAction = userAction });
            }
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Envelope") return;
            InitializeViewport();
            _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            var errorMessage = "";

            ((Activity)Context).RunOnUiThread(new Runnable(() =>
                {

                    if (e == null)
                    {
                        errorMessage = "Unexpected error: DataChangedEventArgs can not be null";
                    }
                    else if (e.Cancelled)
                    {
                        errorMessage = "Cancelled";
                    }
                    else if (e.Error is System.Net.WebException)
                    {
                        errorMessage = "WebException: " + e.Error.Message;
                    }
                    else if (e.Error != null)
                    {
                        errorMessage = e.Error.GetType() + ": " + e.Error.Message;
                    }
                    else // no problems
                    {
                        RefreshGraphics();
                    }
                    //todo show toast with errorMessage
                }));
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
            if (_renderer.Canvas == null)
                _renderer.Canvas = canvas;

            _renderer.Render(_viewport, _map.Layers);
        }
    }

    public class ViewChangedEventArgs : EventArgs
    {
        public Viewport Viewport { get; set; }
        public bool UserAction { get; set; }
    }
}