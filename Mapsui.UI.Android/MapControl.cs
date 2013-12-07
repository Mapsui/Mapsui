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
        private Point _previousMousePosition;

        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public void Initialize()
        {
            Map = new Map();
            _renderer = new MapRenderer();
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
            _viewport.Width = Width;
            _viewport.Height = Height;

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
                    break;
                case MotionEventActions.Up:
                    break;
                case MotionEventActions.PointerDown:
                    break;
                case MotionEventActions.PointerUp:
                    break;
                case MotionEventActions.Move:
                    var currentMousePosition = new Point(x, y);
                    if (_previousMousePosition != null)
                    {
                        _viewport.Transform(currentMousePosition.X, currentMousePosition.Y, _previousMousePosition.X,
                                            _previousMousePosition.Y);
                        _map.ViewChanged(false, _viewport.Extent, _viewport.Resolution);
                    }
                    _previousMousePosition = currentMousePosition;
                    break;
            }

            _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
            Invalidate();
            _previousMousePosition = new Point(x, y);
            return true;
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
            if (_map != null)
            {
                if (ViewChanged != null)
                {
                    ViewChanged(this, new ViewChangedEventArgs { Viewport = _viewport, UserAction = userAction });
                }
            }
        }

        void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Envelope")
            {
                InitializeViewport();
                _map.ViewChanged(true, _viewport.Extent, _viewport.Resolution);
            }
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            string errorMessage = "";

            ((Activity)Context).RunOnUiThread(new Runnable(() =>
                {

                    if (e == null)
                    {
                        errorMessage = "Unexpected error: DataChangedEventArgs can not be null";
                        //OnErrorMessageChanged(EventArgs.Empty);
                    }
                    else if (e.Cancelled)
                    {
                        errorMessage = "Cancelled";
                        //OnErrorMessageChanged(EventArgs.Empty);
                    }
                    else if (e.Error is System.Net.WebException)
                    {
                        errorMessage = "WebException: " + e.Error.Message;
                        //OnErrorMessageChanged(EventArgs.Empty);
                    }
                    else if (e.Error != null)
                    {
                        errorMessage = e.Error.GetType() + ": " + e.Error.Message;
                        //OnErrorMessageChanged(EventArgs.Empty);
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
            Invalidate();
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return; //stop if the line above failed. 

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