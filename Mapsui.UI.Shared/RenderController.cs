#pragma warning disable IDE0005 // Using directive is unnecessary.
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static NetTopologySuite.Geometries.Utilities.GeometryMapper;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace Mapsui.Rendering;

public sealed class RenderController : IDisposable
{
    private bool _disposed;
    private readonly Action? _invalidateCanvas; // Action to call for a redraw of the control
    private readonly int _minimumTimeBetweenInvalidates = 4; // The minimum time in between invalidate calls in ms.
    private readonly int _minimumTimeBetweenStartOfDrawCall = 8; // The minimum time in between the start of two draw calls in ms
    private readonly AsyncAutoResetEvent _isDrawingDone = new(true);
    private readonly AsyncAutoResetEvent _needsRefresh = new(true);
    private static bool _firstDraw = true;
    private bool _isRunning = true;
    private int _timestampStartDraw;
    private readonly Stopwatch _stopwatch = new(); // Stopwatch for measuring drawing times
    // Use the explicit factory if one was configured (e.g. by SampleConfiguration.ApplyRendererConfig());
    // otherwise fall back to direct construction which triggers MapRenderer's static ctor and
    // registers the standard Skia renderer as the default factory.
    private IMapRenderer _mapRenderer = DefaultRendererFactory.IsConfigured ? DefaultRendererFactory.Create() : new MapRenderer();
    private readonly Func<Map?> _getMap;
    // Pending refresh request: null = nothing pending yet, otherwise accumulates since the last render.
    private RefreshRequest? _pendingRefresh;
    private readonly object _refreshLock = new();

    public RenderController(Func<Map?> getMap, Action InvalidateCanvas)
    {
        _getMap = getMap; // Using a callback to get the Map instead of a pointer because the Map field change later on.
        _invalidateCanvas = InvalidateCanvas;
        _timestampStartDraw = GetTimestampInMilliseconds();
        Catch.TaskRun(InvalidateLoopAsync);
    }

    public void SetMapRenderer(IMapRenderer mapRenderer) => _mapRenderer = mapRenderer;

    /// <summary>
    /// Sets up the drawable factory on the RenderService. This wires the renderer's
    /// <see cref="IMapRenderer.CreateDrawableForFeature"/> method to the
    /// <see cref="RenderService.CreateDrawable"/> delegate, enabling the fetch pipeline
    /// to create drawables without direct access to the renderer.
    /// </summary>
    /// <param name="renderService">The render service to configure.</param>
    public void SetupDrawableFactory(RenderService renderService)
    {
        renderService.CreateDrawable = _mapRenderer.CreateDrawableForFeature;
    }

    /// <summary>
    /// Delegates to the map renderer's UpdateDrawables to create pre-rendered objects.
    /// Called when layer data changes.
    /// </summary>
    public void UpdateDrawables(Viewport viewport, ILayer layer, RenderService renderService)
    {
        _mapRenderer.UpdateDrawables(viewport, layer, renderService);
    }

    /// <summary>
    /// Signals that the entire viewport needs to be redrawn on the next render cycle.
    /// </summary>
    public void RefreshGraphics()
    {
        lock (_refreshLock)
            _pendingRefresh = _pendingRefresh == null ? RefreshRequest.Full : _pendingRefresh.Accumulate(RefreshRequest.Full);
        _needsRefresh.Set();
    }

    /// <summary>
    /// Signals that only the given region needs to be redrawn.
    /// Multiple calls before the next render are accumulated.
    /// If a full refresh is already pending, the incoming request is ignored.
    /// </summary>
    /// <param name="request">The refresh request, or <see langword="null"/> to force a full refresh.</param>
    public void RefreshGraphics(RefreshRequest? request)
    {
        var incoming = request ?? RefreshRequest.Full;
        lock (_refreshLock)
            _pendingRefresh = _pendingRefresh == null ? incoming : _pendingRefresh.Accumulate(incoming);
        _needsRefresh.Set();
    }

    // Atomically take and reset the pending refresh. Returns null when nothing is pending.
    private RefreshRequest? TakePendingRefresh()
    {
        lock (_refreshLock)
        {
            var r = _pendingRefresh;
            _pendingRefresh = null;
            return r;
        }
    }

    public MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService,
        Mapsui.Styles.Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
    {
        return _mapRenderer.RenderToBitmapStream(viewport, layers, renderService, background, pixelDensity, widgets, renderFormat, quality);
    }

    public MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0)
    {
        return _mapRenderer.GetMapInfo(screenPosition, viewport, layers, renderService, margin);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _isRunning = false;
        _needsRefresh.Set();
        _isDrawingDone.Set();

        _disposed = true;
    }

    private async Task InvalidateLoopAsync()
    {
        while (_isRunning)
        {
            // What is happening here?
            // - Always wait for the previous draw to finish, so there are no dropped frames anymore. By waiting the
            // loop update frequency can adapt to longer drawing durations.
            // - After that always wait for 8 ms so that the process is never 100% busy drawing, even when drawing 
            // takes long.
            // - Then depending on how long drawing took we either don't wait (when 16 ms have already passed)
            // or wait until 16 ms have elapsed since the previous start of drawing. The previous delay is taken into account
            // so the wait will be between 0 and 8 ms depending on how long the previous draw took.
            // - Then wait for _needsRefresh to be Set. If it was already Set it won't wait.

            await _isDrawingDone.WaitAsync().ConfigureAwait(false); // Wait for previous Draw to finish.
            Thread.Sleep(_minimumTimeBetweenInvalidates); // Always wait at least some period in between Draw and Invalidate calls.
            Thread.Sleep(GetAdditionalTimeToDelay(_timestampStartDraw, _minimumTimeBetweenStartOfDrawCall)); // Wait to enforce the _minimumTimeBetweenStartOfDrawCall.
            await _needsRefresh.WaitAsync().ConfigureAwait(false); // Wait if there was no call to _needsRefresh.Set() yet.

            var isAnimating = UpdateAnimations(_getMap());

            _invalidateCanvas?.Invoke();

            if (isAnimating)
                _needsRefresh.Set(); // While still animating trigger another loop. 
        }
    }

    /// <summary>
    /// Renders the map to the given canvas, applying the supplied pixel density scaling first.
    /// If <paramref name="pixelDensity"/> is <see langword="null"/> the view is not yet laid out;
    /// the render is skipped but the invalidation loop is still unblocked so it can retry later.
    /// </summary>
    public void Render(object canvas, float? pixelDensity)
    {
        try
        {
            if (pixelDensity is null)
                return;
            if (_mapRenderer is null)
                return;
            if (_getMap() is not Map map)
                return;
            if (!map.Navigator.Viewport.HasSize())
                return;

            if (_firstDraw)
            {
                _firstDraw = false;
                Logger.Log(LogLevel.Information, $"First Render cycle.");
                Logger.Log(LogLevel.Information, $"{nameof(LoggingWidget)}.{nameof(LoggingWidget.ShowLoggingInMap)} is set to '{nameof(ActiveMode)}.{LoggingWidget.ShowLoggingInMap}'.");
                Logger.Log(LogLevel.Information, $"If you need to remove it in debug mode set: {nameof(LoggingWidget)}.{nameof(LoggingWidget.ShowLoggingInMap)} = {nameof(ActiveMode)}.{ActiveMode.No}.");
            }

            // Start stopwatch before updating animations and drawing control
            _stopwatch.Restart();
            _timestampStartDraw = GetTimestampInMilliseconds();

            ((SKCanvas)canvas).Scale(pixelDensity.Value, pixelDensity.Value);

            var pending = TakePendingRefresh();
            _mapRenderer.Render(canvas, map.Navigator.Viewport, map.Layers, map.Widgets, map.RenderService, map.BackColor, pending?.DirtyRect, pending?.CoordinateSpace ?? CoordinateSpace.World);

            _stopwatch.Stop();

            // If we are interested in performance measurements, we save the new drawing time
            map.Performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);
        }
        finally
        {
            // Always unblock the invalidation loop, even when rendering was skipped.
            // Without this, a skipped paint event (e.g. view not yet laid out) permanently
            // stalls the loop because it waits for _isDrawingDone which would never be set.
            _isDrawingDone.Set();
        }
    }

    /// <summary>
    /// Renders the map to the given canvas without applying any pixel density scaling.
    /// Use this overload from platforms that apply DPI scaling before providing the canvas
    /// (e.g. Avalonia), or from callers that have already scaled the canvas.
    /// </summary>
    public void Render(object canvas) => Render(canvas, 1f);

    private static int GetTimestampInMilliseconds()
    {
        return (int)(Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency);
    }

    private static int GetAdditionalTimeToDelay(int timestampStartDraw, int minimumTimeBetweenStartOfDrawCall)
    {
        var timeSinceLastDraw = GetTimestampInMilliseconds() - timestampStartDraw;
        var additionalTimeToDelay = Math.Max(minimumTimeBetweenStartOfDrawCall - timeSinceLastDraw, 0);
        return additionalTimeToDelay;
    }

    private static bool UpdateAnimations(Map? map)
    {
        var isAnimating = false;
        if (map is Map localMap)
        {
            if (localMap.UpdateAnimations()) // Update animations on the Map
                isAnimating = true;
            if (localMap.Navigator.UpdateAnimations()) // Update animations on the Navigator
                isAnimating = true;
        }
        return isAnimating; // Returns true if there are active animations.
    }

}
