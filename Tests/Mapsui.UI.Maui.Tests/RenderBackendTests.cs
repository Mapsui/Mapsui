// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Mapsui.Manipulations;
using Mapsui.UI.Maui.Drawing;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Mapsui.UI.Maui.Tests;

#pragma warning disable IDISP016, IDISP017 // These tests deliberately exercise repeated disposal and late callbacks.

[TestFixture, NonParallelizable]
public class RenderBackendTests
{
    private RenderBackend _original;
    private IDispatcherProvider? _originalDispatcher;

    [SetUp]
    public void SetUp()
    {
        _originalDispatcher = DispatcherProvider.Current;
        DispatcherProvider.SetCurrent(new TestDispatcherProvider());
        _original = MapControl.Backend;
    }

    [TearDown]
    public void TearDown()
    {
        MapControl.Backend = _original;
        DispatcherProvider.SetCurrent(_originalDispatcher);
    }

    [Test]
    public void CompatibilityPropertyRoundTrips()
    {
        MapControl.UseGPU = false;
        Assert.That(MapControl.Backend, Is.EqualTo(RenderBackend.Software));
        MapControl.Backend = RenderBackend.AndroidSurfaceView;
        Assert.That(MapControl.UseGPU, Is.True);
        MapControl.UseGPU = true;
        Assert.That(MapControl.Backend, Is.EqualTo(RenderBackend.Gpu));
        MapControl.Backend = RenderBackend.Software;
        Assert.That(MapControl.UseGPU, Is.False);
    }

    [TestCase(RenderBackend.Software, typeof(SKCanvasView))]
    [TestCase(RenderBackend.Gpu, typeof(SKGLView))]
    public void DefaultIsSnapshottedAtConstruction(RenderBackend backend, Type viewType)
    {
        MapControl.Backend = backend;
        using var control = new MapControl();
        MapControl.Backend = backend == RenderBackend.Gpu ? RenderBackend.Software : RenderBackend.Gpu;
        Assert.That(control.ActiveBackend, Is.EqualTo(backend));
        Assert.That(control.Content, Is.TypeOf(viewType));
    }

    [Test]
    public void ExplicitSelectionIgnoresStaticDefault()
    {
        MapControl.Backend = RenderBackend.Software;
        using var control = new MapControl(RenderBackend.Gpu);
        Assert.That(control.ActiveBackend, Is.EqualTo(RenderBackend.Gpu));
        Assert.That(control.Content, Is.TypeOf<SKGLView>());
        Assert.That(MapControl.Backend, Is.EqualTo(RenderBackend.Software));
    }

    [Test]
    public void AndroidSelectionMapsToGpuOnWindows()
    {
        using var control = new MapControl(RenderBackend.AndroidSurfaceView);
        Assert.That(control.ActiveBackend, Is.EqualTo(RenderBackend.Gpu));
        Assert.That(control.Content, Is.TypeOf<SKGLView>());
    }

    [Test]
    public void UnknownBackendIsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() =>
    {
        using var control = new MapControl((RenderBackend)999);
    });

    [Test]
    public void DisposeClearsContentAndIsIdempotent()
    {
        using var control = new MapControl(RenderBackend.Software);
        control.Dispose();
        control.Dispose();
        control.InvalidateCanvas();
        Assert.That(control.Content, Is.Null);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void TouchPixelsAreScaledExactlyOnce(float density)
    {
        var position = MapControl.GetScreenPosition(new SKPoint(120 * density, 75 * density), density);
        Assert.That(position.X, Is.EqualTo(120));
        Assert.That(position.Y, Is.EqualTo(75));
    }

    [Test]
    public void CancelledTouchDoesNotLeaveAGhostPointer()
    {
        using var control = new MapControl(RenderBackend.Gpu);
        control.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, 100, 100));
        var view = (ISKGLView)control.Content;
        view.OnCanvasSizeChanged(new SKSizeI(200, 200));
        var presses = new List<ScreenPosition>();
        control.MapPointerPressed += (_, args) => presses.Add(args.ScreenPosition);
        view.OnTouch(new SKTouchEventArgs(1, SKTouchAction.Pressed, new SKPoint(40, 80), true));
        view.OnTouch(new SKTouchEventArgs(1, SKTouchAction.Cancelled, new SKPoint(40, 80), false));
        view.OnTouch(new SKTouchEventArgs(2, SKTouchAction.Pressed, new SKPoint(40, 80), true));
        Assert.That(presses, Has.Count.EqualTo(2));
        Assert.That(presses[0], Is.EqualTo(new ScreenPosition(20, 40)));
        Assert.That(presses[1], Is.EqualTo(new ScreenPosition(20, 40)));
    }

    [Test]
    public void DisposedAdapterStopsPaintAndTouchCallbacks()
    {
        using var drawing = new TestDrawingView();
        using var surface = SKSurface.Create(new SKImageInfo(4, 4));
        var paints = 0;
        var touches = 0;
        drawing.Paint += (_, _) => paints++;
        drawing.Touch += (_, _) => touches++;
        drawing.Emit(surface.Canvas);
        drawing.Dispose();
        drawing.Emit(surface.Canvas);
        Assert.That(paints, Is.EqualTo(1));
        Assert.That(touches, Is.EqualTo(1));
        Assert.That(drawing.DetachCount, Is.EqualTo(1));
    }

    private sealed class TestDrawingView() : MapDrawingView(new ContentView())
    {
        public int DetachCount { get; private set; }
        public override SKSize CanvasSize => new(4, 4);
        public override void Invalidate() { }
        protected override void Detach() => DetachCount++;
        public void Emit(SKCanvas canvas)
        {
            OnPaint(canvas, null);
            OnTouch(this, new SKTouchEventArgs(7, SKTouchAction.Cancelled, new SKPoint(2, 3), false));
        }
    }

    // These are control-construction tests, not WinUI application or native host tests.
    private sealed class TestDispatcherProvider : IDispatcherProvider, IDispatcher
    {
        public IDispatcher GetForCurrentThread() => this;
        public bool IsDispatchRequired => false;
        public bool Dispatch(Action action) { action(); return true; }
        public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();
        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
    }
}
