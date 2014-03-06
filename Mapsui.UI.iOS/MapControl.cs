using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Math = System.Math;
using System.Drawing;
using Mapsui.Rendering.iOS;
using System.ComponentModel;
using Mapsui.Fetcher;
using MonoTouch.CoreFoundation;

namespace Mapsui.UI.iOS
{
    [Register("MapControl")]
    public class MapControl : UIView
    {
        public delegate void ViewportInitializedEventHandler(object sender);
        public event ViewportInitializedEventHandler ViewportInitializedEvent;

        private const int None = 0;
        private const int Drag = 1;
        private const int Zoom = 2;
        //private int _mode = None;
        //private PointF _previousMap, _currentMap;
        private PointF _previousMid = new PointF();
        private PointF _currentMid = new PointF();
        private float _oldDist = 1f;

        private bool _viewportInitialized;
        public bool ViewportInitialized
        {
            get { return _viewportInitialized; }
            set
            {
                _viewportInitialized = value;
                if (_viewportInitialized && ViewportInitializedEvent != null)
                    ViewportInitializedEvent(this);
            }
        }

        private float Width { get { return this.Frame.Width; } }
        private float Height { get { return this.Frame.Height; } }

        public MapControl(IntPtr handle)
            : base(handle)
        {
            Initialize();
        }

        public MapControl(RectangleF frame)
            : base(frame)
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
            _renderer = new MapRenderer(this);
            InitializeViewport();

            this.ClipsToBounds = true;

            var pinchGesture = new UIPinchGestureRecognizer(PinchGesture);
            pinchGesture.Enabled = true;
            this.AddGestureRecognizer(pinchGesture);
        }

        private void InitializeViewport()
        {
            if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
            if (_map == null) return;
            if (_map.Envelope == null) return;
            if (Math.Abs(_map.Envelope.Width - 0d) < Utilities.Constants.Epsilon) return;
            if (Math.Abs(_map.Envelope.Height - 0d) < Utilities.Constants.Epsilon) return;
            if (_map.Envelope.GetCentroid() == null) return;

            if (double.IsNaN(_map.Viewport.Resolution) || double.IsInfinity(_map.Viewport.Resolution))
                _map.Viewport.Resolution = _map.Envelope.Width / Width;
            if ((double.IsNaN(_map.Viewport.CenterX)) || double.IsNaN(_map.Viewport.CenterY) ||
                double.IsInfinity(_map.Viewport.CenterX) || double.IsInfinity(_map.Viewport.CenterY))
                _map.Viewport.Center = _map.Envelope.GetCentroid();

            _map.Viewport.Width = Width;
            _map.Viewport.Height = Height;
            _map.Viewport.RenderScaleFactor = 2;

            _map.ViewChanged(true, _map.Viewport.Extent, _map.Viewport.RenderResolution);
            _viewportInitialized = true;
        }
        private void PinchGesture(UIPinchGestureRecognizer recognizer)
        {
            if (recognizer.NumberOfTouches < 2)
                return;

            if (recognizer.State == UIGestureRecognizerState.Began)
            {
                _oldDist = 1;
                _currentMid = recognizer.LocationInView(this);
            }

            float scale = 1 - (_oldDist - recognizer.Scale);

            if (scale > 0.5 && scale < 1.5)
            {
                if (_oldDist != recognizer.Scale)
                {
                    _oldDist = recognizer.Scale;
                    _currentMid = recognizer.LocationInView(this);
                    _previousMid = new PointF(_currentMid.X, _currentMid.Y);

                    _map.Viewport.Center = _map.Viewport.ScreenToWorld(
                        _currentMid.X,
                        _currentMid.Y);
                    _map.Viewport.Resolution = _map.Viewport.Resolution / scale;
                    _map.Viewport.Center = _map.Viewport.ScreenToWorld(
                        (_map.Viewport.Width - _currentMid.X),
                        (_map.Viewport.Height - _currentMid.Y));
                }

                _map.Viewport.Transform(
                    _currentMid.X,
                    _currentMid.Y,
                    _previousMid.X,
                    _previousMid.Y);

                RefreshGraphics();
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            MapHelper.OnTouchDown(touches, evt);
        }

        public override void TouchesMoved(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            if (touches.Count == 1)
            {
                var touch = touches.AnyObject as UITouch;
                var currentPos = touch.LocationInView(this);
                var previousPos = touch.PreviousLocationInView(this);

                var cRect = new Rectangle(new Point((int)currentPos.X, (int)currentPos.Y), new Size(5, 5));
                var pRect = new Rectangle(new Point((int)previousPos.X, (int)previousPos.Y), new Size(5, 5));

                if (!cRect.IntersectsWith(pRect))
                {
                    _map.Viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);
                    RefreshGraphics();
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            //base.TouchesEnded (touches, evt);
            RefreshGraphics();
            _map.ViewChanged(false, _map.Viewport.Extent, _map.Viewport.Resolution);
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
            System.Diagnostics.Debug.WriteLine("MapPropertyChanged = InitializeViewport");
            InitializeViewport();
            _map.ViewChanged(true, _map.Viewport.Extent, _map.Viewport.RenderResolution);
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            var errorMessage = "";

            DispatchQueue.MainQueue.DispatchSync(delegate
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
            });
        }

        private void RefreshGraphics()
        {
            SetNeedsDisplay();
        }

        public override void Draw(RectangleF rect)
        {

            base.Draw(rect);
            if (!ViewportInitialized)
            {
                System.Diagnostics.Debug.WriteLine("Draw = InitializeViewport");
                InitializeViewport();
            }
            if (!ViewportInitialized)
                return;

            _renderer.Render(_map.Viewport, _map.Layers);
        }
    }
}