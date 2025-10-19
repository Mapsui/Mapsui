#pragma warning disable IDE0005 // Using directive is unnecessary.
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
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
    private IMapRenderer _mapRenderer = new MapRenderer();
    private readonly Func<Map?> _getMap;

    public RenderController(Func<Map?> getMap, Action InvalidateCanvas)
    {
        _getMap = getMap; // Using a callback to get the Map instead of a pointer because the Map field change later on.
        _invalidateCanvas = InvalidateCanvas;
        _timestampStartDraw = GetTimestampInMilliseconds();
        Catch.TaskRun(InvalidateLoopAsync);
    }

    public void SetMapRenderer(IMapRenderer mapRenderer) => _mapRenderer = mapRenderer;

    public void RefreshGraphics()
    {
        _needsRefresh.Set();
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

    public void Render(object canvas)
    {
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

        _mapRenderer.Render(canvas, map.Navigator.Viewport, map.Layers, map.Widgets, map.RenderService, map.BackColor);

        _isDrawingDone.Set();
        _stopwatch.Stop();

        // If we are interested in performance measurements, we save the new drawing time
        map.Performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);
    }

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
