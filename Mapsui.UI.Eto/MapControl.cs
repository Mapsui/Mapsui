
namespace Mapsui.UI.Eto;

using System;
using Mapsui.Utilities;
using Mapsui.Rendering.Skia;
using Mapsui.UI.Eto.Extensions;
using global::Eto.SkiaDraw;
using global::Eto.Drawing;
using global::Eto.Forms;
using System.Diagnostics;
using Mapsui.Extensions;
using NetTopologySuite.GeometriesGraph;

public partial class MapControl : SkiaDrawable, IMapControl
{
    private RectangleF _selectRectangle = new();
    private PointF? _downMousePosition;
    private Cursor _defaultCursor = Cursors.Default;
    public Cursor MoveCursor { get; set; } = Cursors.Move;
    public MouseButtons MoveButton { get; set; } = MouseButtons.Primary;
    public Keys MoveModifier { get; set; } = Keys.None;
    public MouseButtons ZoomButton { get; set; } = MouseButtons.Primary;
    public Keys ZoomModifier { get; set; } = Keys.Control;

    public MapControl()
    {
        CommonInitialize();
        ControlInitialize();
    }

    private void ControlInitialize()
    {
        _invalidate = () => RunOnUIThread(Invalidate);

        // Mapsui.Rendering.Skia use Mapsui.Nts where GetDbaseLanguageDriver need encoding providers
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        Renderer = new MapRenderer();
        RefreshGraphics();

        Content = CreateBoundingBoxDrawable();

        SizeChanged += (s, e) => SetViewportSize();
    }

    private Drawable CreateBoundingBoxDrawable()
    {
        var drawable = new Drawable { Visible = false };

        drawable.Paint += (o, e) =>
        {
            var fill = new Color(Colors.Yellow, 0.4f);

            using var border = Pens.Cached(Colors.Black, 1.4f, DashStyles.Dash);

            e.Graphics.FillRectangle(fill, _selectRectangle);
            e.Graphics.DrawRectangle(border, _selectRectangle);
        };

        return drawable;
    }

    protected override void OnLoadComplete(EventArgs e)
    {
        base.OnLoadComplete(e);

        SetViewportSize();

        CanFocus = true;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        var mouseWheelDelta = (int)e.Delta.Height;
        var currentMousePosition = e.Location.ToMapsui();
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        SetViewportSize();
    }

    private void RunOnUIThread(Action action)
    {
        Application.Instance.AsyncInvoke(action);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        IsInBoxZoomMode = e.Buttons == ZoomButton && (ZoomModifier == Keys.None || e.Modifiers == ZoomModifier);

        bool move_mode = e.Buttons == MoveButton && (MoveModifier == Keys.None || e.Modifiers == MoveModifier);

        if (move_mode)
            _defaultCursor = Cursor;

        if (move_mode || IsInBoxZoomMode)
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
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (IsInBoxZoomMode)
        {
            var previous = Map.Navigator.Viewport.ScreenToWorld(_selectRectangle.TopLeft.X, _selectRectangle.TopLeft.Y);
            var current = Map.Navigator.Viewport.ScreenToWorld(_selectRectangle.BottomRight.X, _selectRectangle.BottomRight.Y);
            ZoomToBox(previous, current);
        }
        else if (_downMousePosition.HasValue)
        {
            if (IsClick(e.Location, _downMousePosition.Value))
                OnInfo(CreateMapInfoEventArgs(e.Location.ToMapsui(), _downMousePosition.Value.ToMapsui(), 1));
        }

        _downMousePosition = null;

        Cursor = _defaultCursor;

        RefreshData();
    }

    private static bool IsClick(PointF currentPosition, PointF previousPosition)
    {
        return Math.Abs(PointF.Distance(currentPosition, previousPosition)) < 5;
    }

    public void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            // The default for this has changed in .net core, you have to explicitly set if to true for it to work.
            UseShellExecute = true
        });
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

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
                Cursor = MoveCursor;

                Map.Navigator.Drag(e.Location.ToMapsui(), _downMousePosition.Value.ToMapsui());
                _downMousePosition = e.Location;
            }
        }
    }

    public void ZoomToBox(MPoint beginPoint, MPoint endPoint)
    {
        var box = new MRect(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y);
        Map.Navigator.ZoomToBox(box, duration: 300); ;
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
