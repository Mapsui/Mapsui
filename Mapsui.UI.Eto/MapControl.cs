
namespace Mapsui.UI.Eto
{
    using System;
    using Mapsui.Utilities;
    using Mapsui.Rendering.Skia;
    using Mapsui.UI.Eto.Extensions;
    using global::Eto.SkiaDraw;
    using global::Eto.Drawing;
    using global::Eto.Forms;
    public partial class MapControl : SkiaDrawable, IMapControl
    {
        private RectangleF _selectRectangle = new();
        private MPoint? _currentMousePosition;
        private MPoint? _downMousePosition;
        private bool _mouseDown;
        private MPoint? _previousMousePosition;
        public MouseWheelAnimation MouseWheelAnimation { get; } = new();
        public MapControl()
        {
            CommonInitialize();
            ControlInitialize();
        }
        private void ControlInitialize()
        {
            _invalidate = () => RunOnUIThread(Invalidate);

            LoadComplete += MapControlLoaded;
            MouseDown += MapControlMouseLeftButtonDown;
            MouseUp += MapControlMouseLeftButtonUp;

            MouseMove += MapControlMouseMove;
            MouseLeave += MapControlMouseLeave;
            MouseWheel += MapControlMouseWheel;

            SizeChanged += MapControlSizeChanged;

            Renderer = new MapRenderer();
            RefreshGraphics();
        }
        private void MapControlLoaded(object sender, EventArgs e)
        {
            SetViewportSize();

            CanFocus = true;
        }
        private void MapControlMouseWheel(object sender, MouseEventArgs e)
        {
            if (_map?.ZoomLock ?? true) return;
            if (!Viewport.HasSize) return;

            _currentMousePosition = e.Location.ToMapsui();

            var resolution = MouseWheelAnimation.GetResolution((int)e.Delta.Height, _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = _map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, _map.Resolutions, _map.Extent);
            Navigator.ZoomTo(resolution, _currentMousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
        }
        private void MapControlSizeChanged(object sender, EventArgs e)
        {
            SetViewportSize();
        }
        private void MapControlMouseLeave(object sender, MouseEventArgs e)
        {
            _previousMousePosition = null;
        }
        private void RunOnUIThread(Action action)
        {
            Application.Instance.AsyncInvoke(action);
        }
        private void MapControlMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.Primary)
                return;

            var touchPosition = e.Location.ToMapsui();
            _previousMousePosition = touchPosition;
            _downMousePosition = touchPosition;
            _mouseDown = true;
        }
        private static bool IsInBoxZoomMode(Keys key)
        {
            return key == Keys.Control || 
                key == Keys.LeftControl || key == Keys.RightControl;
        }
        private void MapControlMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.Primary)
                return;

            var mousePosition = e.Location.ToMapsui();

            if (_previousMousePosition != null)
            {
                if (IsInBoxZoomMode(e.Modifiers))
                {
                    var previous = Viewport.ScreenToWorld(_previousMousePosition.X, _previousMousePosition.Y);
                    var current = Viewport.ScreenToWorld(mousePosition.X, mousePosition.Y);
                    ZoomToBox(previous, current);
                }
                else if (_downMousePosition != null && IsClick(mousePosition, _downMousePosition))
                {
                    OnInfo(InvokeInfo(mousePosition, _downMousePosition, 1));
                }
            }

            RefreshData();
            _mouseDown = false;

            _previousMousePosition = new MPoint();
        }
        private static bool IsClick(MPoint currentPosition, MPoint previousPosition)
        {
            return
                Math.Abs(currentPosition.X - previousPosition.X) < 5 &&
                Math.Abs(currentPosition.Y - previousPosition.Y) < 5;
        }
        public void OpenBrowser(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
        private void MapControlMouseMove(object sender, MouseEventArgs e)
        {
            if (IsInBoxZoomMode(e.Modifiers))
            {
                DrawBbox(e.Location);
                return;
            }

            _currentMousePosition = e.Location.ToMapsui(); //Needed for both MouseMove and MouseWheel event

            if (_mouseDown)
            {
                if (_previousMousePosition == null)
                {
                    // Usually MapControlMouseLeftButton down initializes _previousMousePosition but in some
                    // situations it can be null. So far I could only reproduce this in debug mode when putting
                    // a breakpoint and continuing.
                    return;
                }

                _viewport.Transform(_currentMousePosition, _previousMousePosition);
                RefreshGraphics();
                _previousMousePosition = _currentMousePosition;
            }
            else
            {
                if (MouseWheelAnimation.IsAnimating())
                {
                    // Disabled because not performing:
                    // Navigator.ZoomTo(_toResolution, _currentMousePosition, _mouseWheelAnimationDuration, Easing.QuarticOut);
                }

            }
        }
        public void ZoomToBox(MPoint beginPoint, MPoint endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                Width, Height, out var x, out var y, out var resolution);

            Navigator.NavigateTo(new MPoint(x, y), resolution, 384);

            RefreshData();
            RefreshGraphics();
            ClearBBoxDrawing();
        }
        private void ClearBBoxDrawing()
        {
            RunOnUIThread(() => Content = null);
        }
        private void DrawBbox(PointF newPos)
        {
            if (_mouseDown)
            {
                if (_previousMousePosition == null) return; // can happen during debug

                var from = _previousMousePosition;
                var to = newPos;

                if (from.X > to.X)
                {
                    var temp = from;
                    from.X = to.X;
                    to.X = (float)temp.X;
                }

                if (from.Y > to.Y)
                {
                    var temp = from;
                    from.Y = to.Y;
                    to.Y = (float)temp.Y;
                }

                _selectRectangle.TopLeft = from.ToEto();
                _selectRectangle.BottomRight = to;

                if (Content is null)
                {
                    var drawable = new Drawable();

                    drawable.Paint += (o, e) =>
                    {
                        var fill = new Color(Colors.Yellow, 0.4f);

                        using var border = Pens.Cached(Colors.Black, 1.5f, DashStyles.Dash);

                        e.Graphics.FillRectangle(fill, _selectRectangle);
                        e.Graphics.DrawRectangle(border, _selectRectangle);
                    };

                    Content = drawable;
                }
                else
                {
                    Content.Invalidate();
                }
            }
        }
        private float ViewportWidth => Width;
        private float ViewportHeight => Height;
        protected override void OnPaint(SKPaintEventArgs e)
        {
            if (PixelDensity <= 0)
                return;

            var canvas = e.Surface.Canvas;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }
        private float GetPixelDensity()
        {
            var center = PointToScreen(Location + Size / 2);

            return Screen.FromPoint(center).LogicalPixelSize;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _map?.Dispose();
            }

#pragma warning disable IDISP023 // Don't use reference types in finalizer context
            CommonDispose(disposing);
#pragma warning restore IDISP023 // Don't use reference types in finalizer context

            base.Dispose(disposing);
        }
    }
}
