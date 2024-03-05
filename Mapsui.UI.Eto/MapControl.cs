using Eto.Drawing;
using Eto.Forms;
using Eto.SkiaDraw;
using Mapsui.Extensions;
using Mapsui.UI.Eto.Extensions;
using System;
using System.Diagnostics;

namespace Mapsui.UI.Eto;

public partial class MapControl : SkiaDrawable, IMapControl
{
    private PointF? _pointerDownPosition;
    private Cursor _defaultCursor = Cursors.Default;
    public MapControl()
    {
        SharedConstructor();
        _invalidate = () => RunOnUIThread(Invalidate);
        SizeChanged += (s, e) => SetViewportSize();
    }

    public Cursor MoveCursor { get; set; } = Cursors.Move;
    public MouseButtons MoveButton { get; set; } = MouseButtons.Primary;
    public Keys MoveModifier { get; set; } = Keys.None;
    public MouseButtons ZoomButton { get; set; } = MouseButtons.Primary;
    public Keys ZoomModifier { get; set; } = Keys.Control;
    private double ViewportWidth => Width;
    private double ViewportHeight => Height;

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            using var process = Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        });
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (IsHovering(e))
            return;

        SetCursorInMoveMode();
        _pointerDownPosition = e.Location;
    }

    private void SetCursorInMoveMode()
    {
        _defaultCursor = Cursor; // And store previous cursor to restore it later
        Cursor = MoveCursor;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_pointerDownPosition.HasValue)
        {
            Map.Navigator.Drag(e.Location.ToMapsui(), _pointerDownPosition.Value.ToMapsui());
            _pointerDownPosition = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        SetCursorInDefaultMode();

        if (_pointerDownPosition.HasValue)
        {
            if (IsTap(e.Location, _pointerDownPosition.Value))
                OnInfo(CreateMapInfoEventArgs(e.Location.ToMapsui(), _pointerDownPosition.Value.ToMapsui(), 1));
        }

        _pointerDownPosition = null;
        RefreshData();
    }

    private void SetCursorInDefaultMode()
    {
        Cursor = _defaultCursor;
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
        var mousePosition = e.Location.ToMapsui();
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePosition);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        SetViewportSize();
    }

    protected override void OnPaint(SKPaintEventArgs e)
    {
        if (PixelDensity <= 0)
            return;

        var canvas = e.Surface.Canvas;
        canvas.Scale(PixelDensity, PixelDensity);
        CommonDrawControl(canvas);
    }

    private double GetPixelDensity()
    {
        var center = PointToScreen(Location + Size / 2);
        return Screen.FromPoint(center).LogicalPixelSize;
    }

    private static void RunOnUIThread(Action action)
    {
        Application.Instance.AsyncInvoke(action);
    }

    private static bool IsTap(PointF currentPosition, PointF previousPosition)
    {
        return Math.Abs(PointF.Distance(currentPosition, previousPosition)) < 5;
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

    private bool IsHovering(MouseEventArgs e)
    {
        return !(e.Buttons == MoveButton && (MoveModifier == Keys.None || e.Modifiers == MoveModifier));
    }
}
