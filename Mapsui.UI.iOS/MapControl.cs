using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using Mapsui.Logging;
using Mapsui.Widgets;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
    [Register("MapControl"), DesignTimeVisible(true)]
    public class MapControl : UIStackView, IMapControl
    {
        private Map _map;
        private readonly MapRenderer _renderer = new MapRenderer();
        private readonly SKGLView _canvas = new SKGLView();
        private nuint _previousTouchCount = 0;
        private nfloat _previousX;
        private nfloat _previousY;
        private double _previousRadius;
        private float _skiaScale;
        private Point _touchDown = new Point();
        private double _previousRotation;
        private double _innerRotation;

        public event EventHandler ViewportInitialized;

        public MapControl(CGRect frame)
            : base(frame)
        {
            Initialize();
        }

        [Preserve]
        public MapControl(IntPtr handle) : base(handle) // used when initialized from storyboard
        {
            Initialize();
        }

        public void Initialize()
        {
            Map = new Map();
            BackgroundColor = UIColor.White;

            _canvas.MultipleTouchEnabled = true;
            AddSubview(_canvas);

            TryInitializeViewport();

            ClipsToBounds = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;

            _canvas.PaintSurface += OnPaintSurface;

            var tapGestureRecognizer = new UITapGestureRecognizer(TapGestureHandler)
            {
                NumberOfTapsRequired = 1,
                CancelsTouchesInView = false,
            };

            AddGestureRecognizer(tapGestureRecognizer);
        }

        private void TapGestureHandler(UITapGestureRecognizer gesture)
        {
            var screenPosition = GetScreenPosition(gesture.LocationInView(this));

            Map.InvokeInfo(screenPosition, screenPosition, _skiaScale, _renderer.SymbolCache, WidgetTouched);
        }

        void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            _map.Viewport.Width = _canvas.Frame.Width;
            _map.Viewport.Height = _canvas.Frame.Height;

            _skiaScale = (float)_canvas.ContentScaleFactor;
            skPaintSurfaceEventArgs.Surface.Canvas.Scale(_skiaScale, _skiaScale);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas,
                _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            if (_map.Viewport.TryInitializeViewport(_map, _canvas.Frame.Width, _canvas.Frame.Height))
            {
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            
            if (evt.AllTouches.Count == 1)
            {
                if (touches.AnyObject is UITouch touch)
                {
                    var currentPos = touch.LocationInView(this);
                    var previousPos = touch.PreviousLocationInView(this);
                    
                    _map.Viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);

                    ViewportLimiter.LimitExtent(_map.Viewport, _map.PanMode, _map.PanLimits, _map.Envelope);

                    RefreshGraphics();
                }
            }
            else if (evt.AllTouches.Count >= 2)
            {
                var prevLocations = evt.AllTouches.Select(t => ((UITouch)t).PreviousLocationInView(this))
                                           .Select(p => new Point(p.X, p.Y)).ToList();
                
                var locations = evt.AllTouches.Select(t => ((UITouch)t).LocationInView(this))
                                        .Select(p => new Point(p.X, p.Y)).ToList();

                var (prevCenter, prevRadius, prevAngle) = GetPinchValues(prevLocations);
                var (center, radius, angle) = GetPinchValues(locations);

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

                ViewportLimiter.Limit(_map.Viewport,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                    _map.PanMode, _map.PanLimits, _map.Envelope);

                RefreshGraphics();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent e)
        {
            Refresh();
        }

        private static (Point centre, double radius, double angle) GetPinchValues(List<Point> locations)
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

            return (new Point(centerX, centerY), radius, angle);
        }
        
        private Point GetScreenPosition(CGPoint point)
        {
            return new Point(point.X * _skiaScale, point.Y * _skiaScale);
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

            DispatchQueue.MainQueue.DispatchAsync(delegate
            {
                try
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
                }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Warning, "Unexpected exception in MapDataChanged", exception);
                    throw;
                }
            });
        }

        public void RefreshGraphics()
        {
            SetNeedsDisplay();
            _canvas?.SetNeedsDisplay();
        }

        public void RefreshData()
        {
            _map?.ViewChanged(true);
        }

        public bool AllowPinchRotation { get; set; }
        public double UnSnapRotationDegrees { get; set; }
        public double ReSnapRotationDegrees { get; set; }

        public Point WorldToScreen(Point worldPosition)
        {
            return SharedMapControl.WorldToScreen(Map.Viewport, _skiaScale, worldPosition);
        }

        public Point ScreenToWorld(Point screenPosition)
        {
            return SharedMapControl.ScreenToWorld(Map.Viewport, _skiaScale, screenPosition);
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                _canvas.Frame = value;
                base.Frame = value;

                if (_map?.Viewport == null) return;

                _map.Viewport.Width = _canvas.Frame.Width;
                _map.Viewport.Height = _canvas.Frame.Height;

                Refresh();
            }
        }

        public override void LayoutMarginsDidChange()
        {
            if (_canvas == null) return;

            base.LayoutMarginsDidChange();

            if (_map?.Viewport == null) return;

            _map.Viewport.Width = _canvas.Frame.Width;
            _map.Viewport.Height = _canvas.Frame.Height;

            Refresh();
        }

        private static void WidgetTouched(IWidget widget, Point screenPosition)
        {
            if (widget is Hyperlink) UIApplication.SharedApplication.OpenUrl(new NSUrl(((Hyperlink)widget).Url));

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