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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Mapsui.UI.Uwp;

namespace Mapsui.UI.Avalonia
{
    public partial class MapControl : Grid, IMapControl
    {
        private Point mousePos;
        private MapsuiCustomDrawOp _drawOp;
        private readonly Rectangle _selectRectangle = CreateSelectRectangle();

        public MouseWheelAnimation MouseWheelAnimation { get; } = new MouseWheelAnimation { Duration = 0 };

        public MapControl()
        {
            ClipToBounds = true;
            Children.Add(_selectRectangle);

            CommonInitialize();
            Initialize();
        }

        void Initialize()
        {
            _invalidate = () => { RunOnUIThread(this.InvalidateVisual); };

            this.Initialized += MapControlInitialized;

            this.PointerWheelChanged += MapControl_PointerWheelChanged;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            switch (change.Property.Name)
            {
                case nameof(this.Bounds):
                    // size changed
                    MapControlSizeChanged();
                    RunOnUIThread(() => Refresh()); // orientation changed;
                    break;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            mousePos = e.GetPosition(this);
        }

        private void OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();
            var tapPosition = mousePos.ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 2));
        }

        private void OnSingleTapped(object? sender, RoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            var tapPosition = mousePos.ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 1));
        }

        public override void Render(DrawingContext context)
        {
            if (_drawOp == null) _drawOp = new MapsuiCustomDrawOp(new Rect(0,0,Bounds.Width,Bounds.Height), this);
            _drawOp.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.Custom(_drawOp);
        }

        private void MapControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (Map.ZoomLock) return;
            if (!Viewport.HasSize) return;

            var currentPoint = e.GetCurrentPoint(this);

            var mousePosition = new Geometries.Point(currentPoint.Position.X, currentPoint.Position.Y);

            var resolution = MouseWheelAnimation.GetResolution(Convert.ToInt32(e.Delta.Length), _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = Map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, Map.Resolutions, Map.Envelope);
            Navigator.ZoomTo(resolution, mousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);

            e.Handled = true;
        }

        private void MapControlInitialized(object? sender, EventArgs eventArgs)
        {
            SetViewportSize();
        }

        private void MapControlSizeChanged()
        {
            SetViewportSize();
        }

        private void RunOnUIThread(Action action)
        {
            Task.Run(() => Dispatcher.UIThread.InvokeAsync(action));
        }

        public void OpenBrowser(string url)
        {
            using (Process process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            }));
        }

        private float ViewportWidth => Convert.ToSingle(Bounds.Width);
        private float ViewportHeight => Convert.ToSingle(Bounds.Height);


        private float GetPixelDensity()
        {
            if (this.VisualRoot != null)
            {
                return Convert.ToSingle(this.VisualRoot.RenderScaling);
            }

            return 1f;
        }

        private static Rectangle CreateSelectRectangle()
        {
            return new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3,
                StrokeDashArray = new AvaloniaList<double>() { 3.0 },
                Opacity = 0.3,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsVisible = false

            };
        }

        private class MapsuiCustomDrawOp : ICustomDrawOperation
        {
            private readonly MapControl mapControl;

            private readonly FormattedText noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };

            public MapsuiCustomDrawOp(Rect bounds, MapControl mapControl)
            {
                Bounds = bounds;
                this.mapControl = mapControl;
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; set; }
            public bool HitTest(Point p)
            {
                return true;
            }
            public bool Equals(ICustomDrawOperation other) => false;
            public void Render(IDrawingContextImpl context)
            {
                var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
                if (canvas == null)
                    context.DrawText(Brushes.Black, new Point(), noSkia.PlatformImpl);
                else
                {
                    canvas.Save();
                    mapControl.CommonDrawControl(canvas);
                    canvas.Restore();
                }
            }
        }
    }
}