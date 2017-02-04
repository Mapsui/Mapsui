using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Utilities;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
    [Register("MapControl"), DesignTimeVisible(true)]
    public class MapControl : SKCanvasView, IMapControl
    {
        private CGPoint _previousMid;
        private CGPoint _currentMid;
        private float _oldDist = 1f;
        private MapRenderer _renderer;
        private Map _map;
        
        private bool _viewportInitialized;

        private float Width => (float)Frame.Width;
        private float Height => (float)Frame.Height;

        public event EventHandler ViewportInitialized;

        public MapControl(CGRect frame)
            : base(frame)
        {
            Initialize();
        }

        public void Initialize()
        {
            Map = new Map();
            if (StartWithOpenStreetMap) Map.Layers.Add(OpenStreetMap.CreateTileLayer());

            BackgroundColor = UIColor.White;
            _renderer = new MapRenderer();

            InitializeViewport();
            
            ClipsToBounds = true;

            var pinchGesture = new UIPinchGestureRecognizer(PinchGesture) { Enabled = true };
            AddGestureRecognizer(pinchGesture);

            PaintSurface += OnPaintSurface;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return;

            if (Width != _map.Viewport.Width) _map.Viewport.Width = Width;
            if (Height != _map.Viewport.Height) _map.Viewport.Height = Height;

            var scaleFactor = 2; // todo: figure out how to get this value programatically
            skPaintSurfaceEventArgs.Surface.Canvas.Scale(scaleFactor, scaleFactor);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas, _map.Viewport, _map.Layers, _map.BackColor);
        }

        private void InitializeViewport()
        {
            if (ViewportHelper.TryInitializeViewport(_map, Width, Height))
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

        private void PinchGesture(UIPinchGestureRecognizer recognizer)
        {
			if (_map.Lock) return;

            if ((int)recognizer.NumberOfTouches < 2)
                return;

            if (recognizer.State == UIGestureRecognizerState.Began)
            {
                _oldDist = 1;
                _currentMid = recognizer.LocationInView(this);
            }

            var scale = 1 - (_oldDist - (float)recognizer.Scale);

            if (scale > 0.5 && scale < 1.5)
            {
                if (_oldDist != (float)recognizer.Scale)
                {
                    _oldDist = (float)recognizer.Scale;
                    _currentMid = recognizer.LocationInView(this);
                    _previousMid = new CGPoint(_currentMid.X, _currentMid.Y);

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

            var majorChange = recognizer.State == UIGestureRecognizerState.Ended;
            _map.ViewChanged(majorChange);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
			if (_map.Lock) return;

            if ((uint)touches.Count == 1)
            {
                var touch = touches.AnyObject as UITouch;
                if (touch != null)
                {
                    var currentPos = touch.LocationInView(this);
                    var previousPos = touch.PreviousLocationInView(this);

                    var cRect = new CGRect(new CGPoint((int)currentPos.X, (int)currentPos.Y), new CGSize(5, 5));
                    var pRect = new CGRect(new CGPoint((int)previousPos.X, (int)previousPos.Y), new CGSize(5, 5));

                    if (!cRect.IntersectsWith(pRect))
                    {
                        _map.Viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);

                        RefreshGraphics();
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent e)
        {
            Refresh();
            HandleInfo(e.AllTouches);
        }

        private void HandleInfo(NSSet touches)
        {
            if (touches.Count != 1) return;
            var touch = touches.FirstOrDefault() as UITouch;
            if (touch == null) return;
            var screenPosition = touch.LocationInView(this);
            Map.InvokeInfo(screenPosition.ToMapsui());
        }

        public void Refresh()
        {
            RefreshGraphics();
            _map.ViewChanged(true);
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
                }

                _map = value;
                
                if (_map != null)
                {
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.RefreshGraphics += MapRefreshGraphics;
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
			if (e.PropertyName == "Enabled")
			{
				RefreshGraphics();
			}
			else if (e.PropertyName == "Opacity")
			{
				RefreshGraphics();
			}
			else if (e.PropertyName == nameof(_map.Envelope))
			{
				InitializeViewport();
				_map.ViewChanged(true);
			}
			else if (e.PropertyName == nameof(_map.Viewport.Rotation)) 
			{
				RefreshGraphics();
				_map.ViewChanged(true);
			}
        }

        private void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            string errorMessage;

            DispatchQueue.MainQueue.DispatchAsync(delegate
            {
                if (e == null)
                {
                    errorMessage = "MapDataChanged Unexpected error: DataChangedEventArgs can not be null";
                    Console.WriteLine(errorMessage);
                }
                else if (e.Cancelled)
                {
                    errorMessage = "MapDataChanged: Cancelled";
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                }
                else if (e.Error is System.Net.WebException)
                {
                    errorMessage = "MapDataChanged WebException: " + e.Error.Message;
                    Console.WriteLine(errorMessage);
                }
                else if (e.Error != null)
                {
                    errorMessage = "MapDataChanged errorMessage: " + e.Error.GetType() + ": " + e.Error.Message;
                    Console.WriteLine(errorMessage);
                }

                RefreshGraphics();
            });
        }

        public void RefreshGraphics()
        {
            SetNeedsDisplay();
        }

        public bool StartWithOpenStreetMap { get; set; }
    }
}