using Mapsui.Fetcher;
using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Widgets;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
    [Register("MapControl"), DesignTimeVisible(true)]
    public partial class MapControl : UIView, IMapControl
    {
        private readonly SKGLView _canvas = new SKGLView();
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

            _scale = GetDeviceIndependentUnits();

            _canvas.TranslatesAutoresizingMaskIntoConstraints = false;
            _canvas.MultipleTouchEnabled = true;

            AddSubview(_canvas);

            AddConstraints(new[] {
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Leading, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Top, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
            });

            TryInitializeViewport();

            ClipsToBounds = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;

            _canvas.PaintSurface += OnPaintSurface;

            var doubleTapGestureRecognizer = new UITapGestureRecognizer(OnDoubleTapped)
            {
                NumberOfTapsRequired = 2,
                CancelsTouchesInView = false,
            };
            AddGestureRecognizer(doubleTapGestureRecognizer);

            var tapGestureRecognizer = new UITapGestureRecognizer(OnSingleTapped)
            {
                NumberOfTapsRequired = 1,
                CancelsTouchesInView = false,
            };
            tapGestureRecognizer.RequireGestureRecognizerToFail(doubleTapGestureRecognizer);
            AddGestureRecognizer(tapGestureRecognizer);
        }

        public float GetDeviceIndependentUnits()
        {
            return (float)_canvas.ContentScaleFactor;
        }

        private void OnDoubleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            var tapWasHandled = Map.InvokeInfo(position, position, 1, Renderer.SymbolCache, WidgetTouched, 2);

            if (!tapWasHandled)
            {
                // TODO 
                // double tap zoom here
            }
        }

        private void OnSingleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            Map.InvokeInfo(position, position, 1, Renderer.SymbolCache, WidgetTouched, 1);
        }

        void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            args.Surface.Canvas.Scale(_scale, _scale);  // we can only set the scale in the render loop

            Renderer.Render(args.Surface.Canvas, _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            if (_map.Viewport.TryInitializeViewport(_map.Envelope, (float)_canvas.Frame.Width, (float)_canvas.Frame.Height))
            {
                Map.RefreshData(true);
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

            _innerRotation = _map.Viewport.Rotation;
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

                    _innerRotation = _map.Viewport.Rotation;
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

                if (!RotationLock)
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

        private Point GetScreenPosition(CGPoint point)
        {
            return new Point(point.X, point.Y);
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
                }
            });
        }

        public void RefreshGraphics()
        {
            SetNeedsDisplay();
            _canvas?.SetNeedsDisplay(); // todo: check if this is needed.
        }

        public void RefreshData()
        {
            _map?.RefreshData(true);
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
            if (widget is Hyperlink hyperlink)
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl(hyperlink.Url));
            }

            widget.HandleWidgetTouched(screenPosition);
        }
        
        public new void Dispose()
        {
            Unsubscribe();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            Unsubscribe();
            base.Dispose(disposing);
        }
    }
}