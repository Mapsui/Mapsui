// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA f

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Mapsui.Utilities;
using SkiaSharp.Views.UWP;
using HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Windows.UI.Xaml.VerticalAlignment;

namespace Mapsui.UI.Uwp
{
    public partial class MapControl : Grid, IMapControl
    {
        private readonly Rectangle _bboxRect = CreateSelectRectangle();
        private readonly SKXamlCanvas _canvas = CreateRenderTarget();
        private double _innerRotation;

        public MapControl()
        {
            Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

            Children.Add(_canvas);
            Children.Add(_bboxRect);

            _canvas.IgnorePixelScaling = true;
            _canvas.PaintSurface += Canvas_PaintSurface;

            Map = new Map();

            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;

            PointerWheelChanged += MapControl_PointerWheelChanged;

            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;
            ManipulationDelta += OnManipulationDelta;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;

            var orientationSensor = SimpleOrientationSensor.GetDefault();
            if (orientationSensor != null)
                orientationSensor.OrientationChanged += (sender, args) => RunOnUIThread(Refresh);
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tabPosition = e.GetPosition(this).ToMapsui();
            OnInfo(InvokeInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Map.Widgets, Viewport, 
                tabPosition, tabPosition, Renderer.SymbolCache, WidgetTouched, 2));
        }

        private void OnSingleTapped(object sender, TappedRoutedEventArgs e)
        {
            var tabPosition = e.GetPosition(this).ToMapsui();
            OnInfo(InvokeInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Map.Widgets, Viewport, 
                tabPosition, tabPosition, Renderer.SymbolCache, WidgetTouched, 1));
        }

        private static Rectangle CreateSelectRectangle()
        {
            return new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3,
                RadiusX = 0.5,
                RadiusY = 0.5,
                StrokeDashArray = new DoubleCollection { 3.0 },
                Opacity = 0.3,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Visibility = Visibility.Collapsed
            };
        }

        private static SKXamlCanvas CreateRenderTarget()
        {
            return new SKXamlCanvas
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent)
            };
        }

        [Obsolete("Use Viewport.ViewportChanged", true)]
#pragma warning disable 67
        public event EventHandler<ViewChangedEventArgs> ViewChanged;
#pragma warning restore 67

        private void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomLock) return;
            if (!Viewport.Initialized) return;

            
            var currentPoint = e.GetCurrentPoint(this);
            //Needed for both MouseMove and MouseWheel event for mousewheel event

            var mousePosition = new Geometries.Point(currentPoint.RawPosition.X, currentPoint.RawPosition.Y);

            var newResolution = DetermineNewResolution(currentPoint.Properties.MouseWheelDelta, Viewport.Resolution);

            // todo: see if we can replace three steps below with Viewport.Transform

            // 1) Temporarily center on the mouse position
            _viewport.Center = Viewport.ScreenToWorld(mousePosition.X, mousePosition.Y);

            // 2) Then zoom 
            _viewport.Resolution = newResolution;

            // 3) Then move the temporary center of the map back to the mouse position
            _viewport.Center = Viewport.ScreenToWorld(
                Viewport.Width - mousePosition.X,
                Viewport.Height - mousePosition.Y);

            e.Handled = true;

            RefreshGraphics();
            RefreshData();
        }

        private double DetermineNewResolution(int mouseWheelDelta, double currentResolution)
        {
            if (mouseWheelDelta > 0)
            {
                var resolution = ZoomHelper.ZoomIn(_map.Resolutions, currentResolution);

                return ViewportLimiter.LimitResolution(resolution, Viewport.Width, Viewport.Height,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions, _map.Envelope);
            }
            if (mouseWheelDelta < 0)
            {
                var resolution = ZoomHelper.ZoomOut(_map.Resolutions, currentResolution);

                return ViewportLimiter.LimitResolution(resolution, Viewport.Width, Viewport.Height,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions, _map.Envelope);
            }
            return currentResolution;
        }
        
        public void RefreshGraphics()
        {
            RunOnUIThread(() => _canvas?.Invalidate());
        }

        public void Clear()
        {
            _map?.ClearCache();
            RefreshGraphics();
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            TryInitializeViewport(ActualWidth, ActualHeight);
            UpdateSize();
        }
        
        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TryInitializeViewport(ActualWidth, ActualHeight);
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            UpdateSize();
            RefreshData();
            Refresh();
        }

        private void UpdateSize()
        {
            _viewport.Width = ActualWidth;
            _viewport.Height = ActualHeight;
        }

        private void RunOnUIThread(Action action)
        {
            Task.Run(() => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()));
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (Renderer == null) return;
            if (_map == null) return;

            TryInitializeViewport(ActualWidth, ActualHeight);
            if (!Viewport.Initialized) return;

            Renderer.Render(e.Surface.Canvas, Map, Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(
                beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                Viewport.Width, Viewport.Height,
                out var x, out var y, out var resolution);

            resolution = ViewportLimiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, _map.ZoomMode, _map.ZoomLimits,
                _map.Resolutions, _map.Envelope);

            _viewport.Resolution = resolution;

            _viewport.Center = new Geometries.Point(x, y);


            RefreshData();
            RefreshGraphics();
            ClearBBoxDrawing();
        }

        private void ClearBBoxDrawing()
        {
            _bboxRect.Margin = new Thickness(0, 0, 0, 0);
            _bboxRect.Width = 0;
            _bboxRect.Height = 0;
        }

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }
        
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var (center, radius, angle) = (e.Position.ToMapsui(), e.Delta.Scale, e.Delta.Rotation);
            var (prevCenter, prevRadius, prevAngle) = (e.Position.ToMapsui().Offset(-e.Delta.Translation.X, -e.Delta.Translation.Y), 1f, 0f);

            double rotationDelta = 0;

            if (!RotationLock)
            {
                _innerRotation += angle - prevAngle;
                _innerRotation %= 360;

                if (_innerRotation > 180)
                    _innerRotation -= 360;
                else if (_innerRotation < -180)
                    _innerRotation += 360;

                if (Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                    rotationDelta = _innerRotation;
                else if (Viewport.Rotation != 0)
                {
                    if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                        rotationDelta = -Viewport.Rotation;
                    else
                        rotationDelta = _innerRotation - Viewport.Rotation;
                }
            }

            Viewport.Transform(center.X, center.Y, prevCenter.X, prevCenter.Y, radius / prevRadius, rotationDelta);

            ViewportLimiter.Limit(_viewport, _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                _map.PanMode, _map.PanLimits, _map.Envelope);
            RefreshGraphics();
            RefreshData(false);
            e.Handled = true;
        }

        public float PixelDensity => (float)DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

        public void OpenBrowser(string url)
        {
            Task.Run(() => Launcher.LaunchUriAsync(new Uri(url)));
        }

        public float ViewportWidth => (float)ActualWidth;
        public float ViewportHeight => (float)ActualHeight;
    }
}