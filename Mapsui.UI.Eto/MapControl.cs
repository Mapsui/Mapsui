using Eto.Drawing;
using Eto.Forms;
using Eto.SkiaDraw;
using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.UI.Eto.Extensions;
using System;
using System.Diagnostics;

namespace Mapsui.UI.Eto;

public partial class MapControl : SkiaDrawable, IMapControl
{
    private Cursor _defaultCursor = Cursors.Default;
    private readonly TapGestureTracker _tapGestureTracker = new();
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl()
    {
        SharedConstructor();
        _invalidate = () => RunOnUIThread(Invalidate);
        SizeChanged += (s, e) => SetViewportSize();
    }

    /// <summary>
    /// The movement allowed between a touch down and touch up in a touch gestures in device independent pixels.
    /// </summary>
    public int MaxTapGestureMovement { get; set; } = 8;
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
        var mouseDownPosition = e.Location.ToMapsui();
        _manipulationTracker.Restart([]); // Todo: This should not have to be empty, but the start touch.
        _tapGestureTracker.SetDownPosition(mouseDownPosition);

        if (OnWidgetPointerPressed(mouseDownPosition, GetShiftPressed()))
            return;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var mouseMovePosition = e.Location.ToMapsui();
        var isHovering = IsHovering(e);
        if (OnWidgetPointerMoved(mouseMovePosition, !isHovering, GetShiftPressed()))
            return;
        if (isHovering)
            return;
        _manipulationTracker.Manipulate([mouseMovePosition], Map.Navigator.Pinch);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        SetCursorInDefaultMode();

        var mouseUpPosition = e.Location.ToMapsui();
        _tapGestureTracker.IfTap((p) =>
        {
            if (OnWidgetTapped(p, 1, GetShiftPressed()))
                return;
            OnInfo(CreateMapInfoEventArgs(p, p, 1));
        }, MaxTapGestureMovement * PixelDensity, mouseUpPosition);

        _manipulationTracker.Manipulate([mouseUpPosition], Map.Navigator.Pinch);
        RefreshData();
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

    protected override void Dispose(bool disposing)
    {
#pragma warning disable IDISP023 // Don't use reference types in finalizer context
        CommonDispose(disposing);
#pragma warning restore IDISP023 // Don't use reference types in finalizer context

        base.Dispose(disposing);
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

    private bool IsHovering(MouseEventArgs e)
    {
        return !(e.Buttons == MoveButton && (MoveModifier == Keys.None || e.Modifiers == MoveModifier));
    }

    private void SetCursorInMoveMode()
    {
        _defaultCursor = Cursor; // And store previous cursor to restore it later
        Cursor = MoveCursor;
    }

    private void SetCursorInDefaultMode()
    {
        Cursor = _defaultCursor;
    }

    private bool GetShiftPressed()
    {
        return false; // Todo: Implement
    }
}
