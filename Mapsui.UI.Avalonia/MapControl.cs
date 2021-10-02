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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Mapsui.UI.Uwp;

namespace Mapsui.UI.Avalonia
{
    public partial class MapControl : Control, IMapControl, ICustomDrawOperation
    {
        private readonly FormattedText noSkia = new FormattedText()
        {
            Text = "Current rendering API is not Skia"
        };

        private Point mousePos;

        public MouseWheelAnimation MouseWheelAnimation { get; } = new MouseWheelAnimation { Duration = 0 };

        public MapControl()
        {
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
            context.Custom(this);
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
            Task.Run(() => Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal));
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

        public void Dispose()
        {
            // No-op
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
            if (canvas == null)
                context.DrawText(Brushes.Black, new Point(), noSkia.PlatformImpl);
            else
            {
                canvas.Save();
                CommonDrawControl(canvas);
                canvas.Restore();
            }
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }

        private float GetPixelDensity()
        {
            if (this.VisualRoot != null)
            {
                return Convert.ToSingle(this.VisualRoot.RenderScaling);
            }

            return 1f;
        }
    }
}