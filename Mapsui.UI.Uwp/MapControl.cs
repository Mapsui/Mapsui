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
using SkiaSharp.Views.UWP;
using HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Windows.UI.Xaml.VerticalAlignment;
using Mapsui.Utilities;

namespace Mapsui.UI.Uwp
{
    public partial class MapControl : Grid, IMapControl
    {
        private readonly Rectangle _selectRectangle = CreateSelectRectangle();
        private readonly SKXamlCanvas _canvas = CreateRenderTarget();
        private double _innerRotation;

        public MouseWheelAnimation MouseWheelAnimation { get; } = new MouseWheelAnimation { Duration = 0 };

        public MapControl()
        {
            Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

            Children.Add(_canvas);
            Children.Add(_selectRectangle);

            _canvas.PaintSurface += Canvas_PaintSurface;

            Map = new Map();

            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;

            PointerWheelChanged += MapControl_PointerWheelChanged;

            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;

            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;

            var orientationSensor = SimpleOrientationSensor.GetDefault();
            if (orientationSensor != null)
                orientationSensor.OrientationChanged += (sender, args) => RunOnUIThread(() => Refresh());
        }


        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            _innerRotation = _viewport.Rotation;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            var tapPosition = e.GetPosition(this).ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 2));
        }

        private void OnSingleTapped(object sender, TappedRoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            var tabPosition = e.GetPosition(this).ToMapsui();
            OnInfo(InvokeInfo(tabPosition, tabPosition, 1));
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
            if (Map.ZoomLock) return;
            if (!Viewport.HasSize) return;

            var currentPoint = e.GetCurrentPoint(this);

            var mousePosition = new Geometries.Point(currentPoint.RawPosition.X, currentPoint.RawPosition.Y);

            var resolution = MouseWheelAnimation.GetResolution(currentPoint.Properties.MouseWheelDelta, _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = Map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, Map.Resolutions, Map.Envelope);
            Navigator.ZoomTo(resolution, mousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);

            e.Handled = true;
        }
        
        public void RefreshGraphics()
        {
            // The commented out code crashes the app when MouseWheelAnimation.Duration > 0. Could be a bug in SKXamlCanvas
            //if (Dispatcher.HasThreadAccess) _canvas?.Invalidate();
            //else RunOnUIThread(() => _canvas?.Invalidate());
            RunOnUIThread(() => _canvas?.Invalidate());
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            SetViewportSize();
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            SetViewportSize();
        }

        private void RunOnUIThread(Action action)
        {
            Task.Run(() => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()));
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (Renderer == null) return;
            if (_map == null) return;
            if (!Viewport.HasSize) return;
            if (PixelDensity <= 0) return;

            e.Surface.Canvas.Scale(PixelDensity, PixelDensity);

            Navigator.UpdateAnimations();
            Renderer.Render(e.Surface.Canvas, new Viewport(Viewport), _map.Layers, _map.Widgets, _map.BackColor);
        }

        [Obsolete("Use MapControl.Navigate.NavigateTo instead", true)]
        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint) { }

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }
        
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            var center = e.Position.ToMapsui();
            var radius = e.Delta.Scale;
            var rotation = e.Delta.Rotation;

            var previousCenter=  e.Position.ToMapsui().Offset(-e.Delta.Translation.X, -e.Delta.Translation.Y);
            var previousRadius = 1f;
            var previousRotation = 0f;

            double rotationDelta = 0;

            if (!Map.RotationLock)
            {
                _innerRotation += rotation - previousRotation;
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

            _viewport.Transform(center, previousCenter, radius / previousRadius, rotationDelta);
            RefreshGraphics();
            e.Handled = true;
        }

        public void OpenBrowser(string url)
        {
            Task.Run(() => Launcher.LaunchUriAsync(new Uri(url)));
        }

        private float ViewportWidth => (float)ActualWidth;
        private float ViewportHeight => (float)ActualHeight;

        private float GetPixelDensity()
        {
            return (float)DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
        }

    }
}