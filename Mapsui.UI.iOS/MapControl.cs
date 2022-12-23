using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Mapsui.UI.iOS.Extensions;
using Mapsui.Utilities;
using SkiaSharp.Views.iOS;
using UIKit;

#nullable enable

namespace Mapsui.UI.iOS
{
    [Register("MapControl"), DesignTimeVisible(true)]
    public partial class MapControl : UIView, IMapControl
    {
        private SKGLView? _glCanvas;
        private SKCanvasView? _canvas;
        private double _virtualRotation;
        private bool _init;

        public static bool UseGPU { get; set; } = true;

        public MapControl(CGRect frame)
            : base(frame)
        {
            CommonInitialize();
            Initialize();
        }

        [Preserve]
        public MapControl(IntPtr handle) : base(handle) // used when initialized from storyboard
        {
            CommonInitialize();
            Initialize();
        }

        private void InitCanvas()
        {
            if (!_init)
            {
                _init = true;
                if (UseGPU)
                {
                    _glCanvas?.Dispose();
                    _glCanvas = new SKGLView();
                }
                else
                {
                    _canvas?.Dispose();
                    _canvas = new SKCanvasView();
                }
            }
        }

        private void Initialize()
        {
            InitCanvas();
            
            _invalidate = () => {
                RunOnUIThread(() => {
                    SetNeedsDisplay();
                    _glCanvas?.SetNeedsDisplay();
                });
            };

            BackgroundColor = UIColor.White;

            if (UseGPU)
            {
                _glCanvas!.TranslatesAutoresizingMaskIntoConstraints = false;
                _glCanvas.MultipleTouchEnabled = true;
                _glCanvas.PaintSurface += OnPaintSurface;
                AddSubview(_glCanvas);

                AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _glCanvas,
                        NSLayoutAttribute.Leading, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _glCanvas,
                        NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _glCanvas,
                        NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _glCanvas,
                        NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });
            }
            else
            {
                _canvas!.TranslatesAutoresizingMaskIntoConstraints = false;
                _canvas.MultipleTouchEnabled = true;
                _canvas.PaintSurface += OnPaintSurface;
                AddSubview(_canvas);

                AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas,
                        NSLayoutAttribute.Leading, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas,
                        NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas,
                        NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas,
                        NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });
            }

            ClipsToBounds = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;

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

            _viewport.SetSize(ViewportWidth, ViewportHeight);
        }


        private void OnDoubleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            OnInfo(InvokeInfo(position, position, 2));
        }

        private void OnSingleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            OnInfo(InvokeInfo(position, position, 1));
        }

        private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
        {
            if (PixelDensity <= 0)
                return;

            var canvas = args.Surface.Canvas;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }
        
        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            if (PixelDensity <= 0)
                return;

            var canvas = args.Surface.Canvas;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }

        public override void TouchesBegan(NSSet touches, UIEvent? evt)
        {
            base.TouchesBegan(touches, evt);

            _virtualRotation = Viewport.Rotation;
        }

        public override void TouchesMoved(NSSet touches, UIEvent? evt)
        {
            base.TouchesMoved(touches, evt);

            if (evt?.AllTouches.Count == 1)
            {
                if (touches.AnyObject is UITouch touch)
                {
                    var position = touch.LocationInView(this).ToMapsui();
                    var previousPosition = touch.PreviousLocationInView(this).ToMapsui();

                    _viewport.Transform(position, previousPosition);
                    RefreshGraphics();

                    _virtualRotation = Viewport.Rotation;
                }
            }
            else if (evt?.AllTouches.Count >= 2)
            {
                var previousLocation = evt.AllTouches.Select(t => ((UITouch)t).PreviousLocationInView(this))
                    .Select(p => new MPoint(p.X, p.Y)).ToList();

                var locations = evt.AllTouches.Select(t => ((UITouch)t).LocationInView(this))
                    .Select(p => new MPoint(p.X, p.Y)).ToList();

                var (previousCenter, previousRadius, previousAngle) = GetPinchValues(previousLocation);
                var (center, radius, angle) = GetPinchValues(locations);

                double rotationDelta = 0;

                if (Map?.RotationLock == false)
                {
                    _virtualRotation += angle - previousAngle;

                    rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                        _virtualRotation, _viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
                }

                _viewport.Transform(center, previousCenter, radius / previousRadius, rotationDelta);
                RefreshGraphics();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent? e)
        {
            Refresh();
        }

        /// <summary>
        /// Gets screen position in device independent units (or DIP or DP).
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private MPoint GetScreenPosition(CGPoint point)
        {
            return new MPoint(point.X, point.Y);
        }

        private void RunOnUIThread(Action action)
        {
            DispatchQueue.MainQueue.DispatchAsync(action);
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                InitCanvas();
                if (UseGPU)
                {
                    _glCanvas!.Frame = value;    
                }
                else
                {
                    _canvas!.Frame = value;
                }
                
                base.Frame = value;
                SetViewportSize();
                OnPropertyChanged();
            }
        }

        public override void LayoutMarginsDidChange()
        {
            InitCanvas();
            if (_glCanvas == null || _canvas == null) return;

            base.LayoutMarginsDidChange();
            SetViewportSize();
        }

        public void OpenBrowser(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }

        public new void Dispose()
        {
            IosCommonDispose(true);
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            IosCommonDispose(disposing);
            base.Dispose(disposing);
        }

        private void IosCommonDispose(bool disposing)
        {
            if (disposing)
            {
                _map?.Dispose();
                Unsubscribe();
                _glCanvas?.Dispose();
                _canvas?.Dispose();
            }

            CommonDispose(disposing);
        }

        private static (MPoint centre, double radius, double angle) GetPinchValues(List<MPoint> locations)
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

            return (new MPoint(centerX, centerY), radius, angle);
        }

        private float ViewportWidth
        {
            get
            {
                InitCanvas();
                if (UseGPU)
                {
                    return (float)_glCanvas!.Frame.Width;    
                }

                return (float)_canvas!.Frame.Width;
                // todo: check if we need _canvas
            }
        }

        private float ViewportHeight
        {
            get
            {
                InitCanvas();
                if (UseGPU)
                {
                    return (float)_glCanvas!.Frame.Height;    
                }

                return (float)_canvas!.Frame.Height;
                // todo: check if we need _canvas
            }
        }

        private float GetPixelDensity()
        {
            InitCanvas();
            if (UseGPU)
            {
                return (float)_glCanvas!.ContentScaleFactor; // todo: Check if I need canvas    
            }

            return (float)_canvas!.ContentScaleFactor; // todo: Check if I need canvas
        }
    }
}