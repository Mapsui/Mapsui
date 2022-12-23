// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

#nullable enable

using System;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.System;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Utilities;
#if __WINUI__
using System.Runtime.Versioning;
using Mapsui.UI.WinUI.Extensions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp.Views.Windows;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
#else
using Mapsui.UI.WinUI.Extensions;
using Windows.Graphics.Display;
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
#endif

#if __WINUI__
#if !HAS_UNO_WINUI
[assembly: SupportedOSPlatform("windows10.0.18362.0")]
#endif
namespace Mapsui.UI.WinUI
#else
namespace Mapsui.UI.Uwp
#endif
{
    public partial class MapControl : Grid, IMapControl, IDisposable
    {
        private readonly Rectangle _selectRectangle = CreateSelectRectangle();
        private readonly SKXamlCanvas _canvas = CreateRenderTarget();
        private double _virtualRotation;

        public MouseWheelAnimation MouseWheelAnimation { get; } = new MouseWheelAnimation { Duration = 0 };

        public MapControl()
        {
            CommonInitialize();
            Initialize();
        }

        private void Initialize()
        {
            _invalidate = () => {
                // The commented out code crashes the app when MouseWheelAnimation.Duration > 0. Could be a bug in SKXamlCanvas
                //if (Dispatcher.HasThreadAccess) _canvas?.Invalidate();
                //else RunOnUIThread(() => _canvas?.Invalidate());
                RunOnUIThread(() => _canvas?.Invalidate());
            };

            Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

            Children.Add(_canvas);
            Children.Add(_selectRectangle);

            _canvas.PaintSurface += Canvas_PaintSurface;

            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;

            PointerWheelChanged += MapControl_PointerWheelChanged;

            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;

            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;

            var orientationSensor = SimpleOrientationSensor.GetDefault();
            if (orientationSensor != null)
                orientationSensor.OrientationChanged += (sender, args) => RunOnUIThread(() => Refresh());
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            RefreshData();
            Console.WriteLine(Guid.NewGuid());
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _virtualRotation = _viewport.Rotation;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tapPosition = e.GetPosition(this).ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 2));
        }

        private void OnSingleTapped(object sender, TappedRoutedEventArgs e)
        {
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

        private void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (_map?.ZoomLock ?? true) return;
            if (!Viewport.HasSize()) return;

            var currentPoint = e.GetCurrentPoint(this);
#if __WINUI__
            var mousePosition = new MPoint(currentPoint.Position.X, currentPoint.Position.Y);
#else
            var mousePosition = new MPoint(currentPoint.RawPosition.X, currentPoint.RawPosition.Y);
#endif
            var resolution = MouseWheelAnimation.GetResolution(currentPoint.Properties.MouseWheelDelta, _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay

            if (this.Map == null)
                return;

            resolution = Map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, Map.Resolutions, Map.Extent);
            Navigator?.ZoomTo(resolution, mousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);

            e.Handled = true;
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
#if __WINUI__
            Catch.TaskRun(() => DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                }
            }));
#else
            Catch.TaskRun(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                }
            }));
#endif
        }

        private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (PixelDensity <= 0)
                return;

            var canvas = e.Surface.Canvas;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var center = e.Position.ToMapsui();
            var radius = e.Delta.Scale;
            var rotation = e.Delta.Rotation;

            var previousCenter = e.Position.ToMapsui().Offset(-e.Delta.Translation.X, -e.Delta.Translation.Y);
            var previousRadius = 1f;

            double rotationDelta = 0;

            if (Map?.RotationLock == false)
            {
                _virtualRotation += rotation; 

                rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                    _virtualRotation, _viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
            }

            _viewport.Transform(center, previousCenter, radius / previousRadius, rotationDelta);
            RefreshGraphics();
            e.Handled = true;
        }

        public void OpenBrowser(string url)
        {
            Catch.TaskRun(async () => await Launcher.LaunchUriAsync(new Uri(url)));
        }

        private float ViewportWidth => (float)ActualWidth;
        private float ViewportHeight => (float)ActualHeight;

        private float GetPixelDensity()
        {
#if __WINUI__
            return (float)(XamlRoot?.RasterizationScale ?? 1f);
#else
            return (float)DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
#endif
        }

#pragma warning disable IDISP023 // Don't use reference types in finalizer context
#if __ANDROID__ 
        protected override void Dispose(bool disposing)
#elif __IOS__ || __MACOS__
        protected new virtual void Dispose(bool disposing)
#else
        protected virtual void Dispose(bool disposing)
#endif
        {
            if (disposing)
            {
                (_canvas as IDisposable)?.Dispose();
#if __IOS__ || __MACOS__ || __ANDROID__ || NETSTANDARD
                (_selectRectangle as IDisposable)?.Dispose();
#endif
                _map?.Dispose();
            }
            CommonDispose(disposing);

#if __ANDROID__ || __IOS__ || __MACOS__
            base.Dispose(disposing);
#endif
        }

#if !(__ANDROID__ )
#if __IOS__ || __MACOS__ || NETSTANDARD || HAS_UNO
        public new void Dispose()
#else 
        public void Dispose()
#endif
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endif
    }
}