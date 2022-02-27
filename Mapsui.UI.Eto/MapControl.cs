
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
        private PointF? _downMousePosition;
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
            MouseWheel += MapControlMouseWheel;

            SizeChanged += MapControlSizeChanged;

            Renderer = new MapRenderer();
            RefreshGraphics();

            Content = CreateBoundingBoxDrawable();
        }
        private Drawable CreateBoundingBoxDrawable()
        {
            var drawable = new Drawable { Visible = false };

            drawable.Paint += (o, e) => {
                var fill = new Color(Colors.Yellow, 0.4f);

                using var border = Pens.Cached(Colors.Black, 1.4f, DashStyles.Dash);

                e.Graphics.FillRectangle(fill, _selectRectangle);
                e.Graphics.DrawRectangle(border, _selectRectangle);
            };

            return drawable;
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

            var resolution = MouseWheelAnimation.GetResolution((int)e.Delta.Height, _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = _map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, _map.Resolutions, _map.Extent);
            Navigator.ZoomTo(resolution, e.Location.ToMapsui(), MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
        }
        private void MapControlSizeChanged(object sender, EventArgs e)
        {
            SetViewportSize();
        }
        private void RunOnUIThread(Action action)
        {
            Application.Instance.AsyncInvoke(action);
        }
        private void MapControlMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.Primary)
                return;

            IsInBoxZoomMode = e.Modifiers == Keys.Control || e.Modifiers == Keys.LeftControl || e.Modifiers == Keys.RightControl;

            _downMousePosition = e.Location;
        }
        private bool IsInBoxZoomMode
        {
            get => Content.Visible;
            set
            {
                _selectRectangle = RectangleF.Empty;
                Content.Visible = value;
            }
        }
        private void MapControlMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (e.Buttons != MouseButtons.Primary)
                return;

            if (IsInBoxZoomMode)
            {
                var previous = Viewport.ScreenToWorld(_selectRectangle.TopLeft.X, _selectRectangle.TopLeft.Y);
                var current = Viewport.ScreenToWorld(_selectRectangle.BottomRight.X, _selectRectangle.BottomRight.Y);
                ZoomToBox(previous, current);
            }
            else if (_downMousePosition.HasValue)
            {
                if (IsClick(e.Location, _downMousePosition.Value))
                    OnInfo(InvokeInfo(e.Location.ToMapsui(), _downMousePosition.Value.ToMapsui(), 1));
            }

            _downMousePosition = null;

            RefreshData();
        }
        private static bool IsClick(PointF currentPosition, PointF previousPosition)
        {
            return Math.Abs(PointF.Distance(currentPosition, previousPosition)) < 5;
        }
        public void OpenBrowser(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
        private void MapControlMouseMove(object sender, MouseEventArgs e)
        {
            if (_downMousePosition.HasValue)
            {
                if (IsInBoxZoomMode)
                {
                    _selectRectangle.TopLeft = PointF.Min(e.Location, _downMousePosition.Value);
                    _selectRectangle.BottomRight = PointF.Max(e.Location, _downMousePosition.Value);
                    Content.Invalidate();
                }
                else // drag/pan - mode
                {
                    _viewport.Transform(e.Location.ToMapsui(), _downMousePosition.Value.ToMapsui());

                    RefreshGraphics();

                    _downMousePosition = e.Location;
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
            RunOnUIThread(() => IsInBoxZoomMode = false);
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
