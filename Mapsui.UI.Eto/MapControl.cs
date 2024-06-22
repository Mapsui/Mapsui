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
    private readonly ManipulationTracker _manipulationTracker = new();
    private bool _shiftPressed;

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

        SetCursorInMoveMode();
        var position = e.Location.ToScreenPosition();

        _manipulationTracker.Restart([position]);

        if (OnMapPointerPressed([position]))
            return;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var isHovering = IsHovering(e);
        var position = e.Location.ToScreenPosition();

        if (OnMapPointerMoved([position], isHovering))
            return;

        if (!isHovering)
            _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        SetCursorInDefaultMode();
        var position = e.Location.ToScreenPosition();
        OnMapPointerReleased([position]);
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
        var mousePosition = e.Location.ToScreenPosition();
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
        if (disposing)
        {
            _map?.Dispose();
        }

#pragma warning disable IDISP023 // Don't use reference types in finalizer context
        CommonDispose(disposing);
#pragma warning restore IDISP023 // Don't use reference types in finalizer context

        base.Dispose(disposing);
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _shiftPressed = e.Shift;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _shiftPressed = e.Shift;
    }

    private double GetPixelDensity()
    {
        var center = PointToScreen(Location + Size / 2);
        return Screen.FromPoint(center).LogicalPixelSize;
    }

    private static void RunOnUIThread(Action action) => Application.Instance.AsyncInvoke(action);

    private bool IsHovering(MouseEventArgs e)
        => !(e.Buttons == MoveButton && (MoveModifier == Keys.None || e.Modifiers == MoveModifier));

    private void SetCursorInMoveMode()
    {
        _defaultCursor = Cursor; // And store previous cursor to restore it later
        Cursor = MoveCursor;
    }

    private void SetCursorInDefaultMode() => Cursor = _defaultCursor;

    private bool GetShiftPressed()
    {
        return _shiftPressed;
    }
}
