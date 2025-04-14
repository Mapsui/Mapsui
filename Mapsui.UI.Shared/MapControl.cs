#pragma warning disable IDE0005 // Disable unused usings. All the #ifs make this hard. Perhaps simplify that first.
#pragma warning disable IDE0055 // Disable fix formatting but this should not be hard to fix
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
using Mapsui.Widgets;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Mapsui.Disposing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mapsui.Manipulations;
using Mapsui.Styles;
using System.Threading.Tasks;
using LogLevel = Mapsui.Logging.LogLevel;

#if __MAUI__
using Microsoft.Maui.Controls;
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __MAPSUI_IOS__
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __AVALONIA__
using Avalonia;
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.UI.Blazor;
#elif __WINDOWSFORMS__
namespace Mapsui.UI.WindowsForms;
#else
namespace Mapsui.UI.Wpf;
#endif

#pragma warning disable IDISP004 // Don't ignore created IDisposable

public partial class MapControl : INotifyPropertyChanged, IDisposable
{
    // Action to call for a redraw of the control
    private protected Action? _invalidate;
    // The minimum time in between invalidate calls in ms.
    private int _minimumTimeBetweenInvalidates = 8;
    // The minimum time in between the start of two draw calls in ms
    private int _minimumTimeBetweenStartOfDrawCall = 16;
    private AsyncCounterEvent signalThatDrawingIsDone = new();
    private static bool _firstDraw = true;
    private bool _isRunning = true;
    private bool _needsRefresh = true;
    private int _timestampStartDraw;
    // Stopwatch for measuring drawing times
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
#pragma warning disable IDISP002 // Is disposed in SharedDispose
    private readonly IRenderer _renderer = new MapRenderer();
#pragma warning restore IDISP002
    private readonly TapGestureTracker _tapGestureTracker = new();
    private readonly FlingTracker _flingTracker = new();
    private double _sharedWidth;
    private double _sharedHeight;

    /// <summary>
    /// The movement allowed between a touch down and touch up in a touch gestures in device independent pixels.
    /// </summary>
#if __WINDOWSFORMS__
    [DefaultValue(8)] // Fix WOF1000 Error
#endif
    public int MaxTapGestureMovement { get; set; } = 8;

    /// <summary>
    /// Use fling gesture to move the map. Default is true. Fling means that the map will continue to move for a 
    /// short time after the user has lifted the finger.
    /// </summary>
#if __WINDOWSFORMS__
    [DefaultValue(true)] // Fix WOF1000 Error
#endif
    public bool UseFling { get; set; } = true;

    /// <summary>
    /// Renderer that is used from this MapControl
    /// </summary>
    public IRenderer Renderer => _renderer;

    /// <summary>
    /// Called whenever the map is clicked. The MapInfoEventArgs contain the features that were hit in
    /// the layers that have IsMapInfoLayer set to true. 
    /// </summary>
    /// <remarks>
    /// The Map.Tapped event is preferred over the Info event. This event is kept for backwards compatibility.
    /// </remarks>
    public event EventHandler<MapInfoEventArgs>? Info;
    /// <summary>
    /// Event that is triggered when the map is tapped. Can be a single tap, double tap or long press.
    /// </summary>
    public event EventHandler<MapEventArgs>? MapTapped;
    /// <summary>
    /// Event that is triggered when on pointer down.
    /// </summary>
    public event EventHandler<MapEventArgs>? MapPointerPressed;
    /// <summary>
    /// Event that is triggered when on pointer move. Can be a drag or hover.
    /// </summary>
    public event EventHandler<MapEventArgs>? MapPointerMoved;
    /// <summary>
    /// Event that is triggered when on pointer up.
    /// </summary>
    public event EventHandler<MapEventArgs>? MapPointerReleased;

    private void SharedConstructor()
    {
        PlatformUtilities.SetOpenInBrowserFunc(OpenInBrowser);
        Map = new Map();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // Mapsui.Rendering.Skia use Mapsui.Nts where GetDbaseLanguageDriver need encoding providers
        _timestampStartDraw = Environment.TickCount;
        Catch.TaskRun(InvalidateLoopAsync);
    }

    private async Task InvalidateLoopAsync()
    {
        while (_isRunning)
        {
            // What is happening here?
            // Always wait for the previous Draw to finish, so there are no dropped frames. This is a way to adapt to the Draw duration.
            // When done always wait for some small duration, currently 8 ms, so that the process is never 100% busy drawing.
            // Then
            // - either wait until the start of the previous Draw is 16 ms ago. The previous delay is taken into account, so the wait will be max 8 ms (16 - 8) with the current settings..
            // - Or start right away if it is already more then 16 ms ago.

            await signalThatDrawingIsDone.WaitAsync(); // Wait for previous Draw to finish.
            await Task.Delay(_minimumTimeBetweenInvalidates).ConfigureAwait(false); // Always wait at least some period in between Draw and Invalidate calls.
            await Task.Delay(GetAdditionalTimeToDelay(_timestampStartDraw, _minimumTimeBetweenStartOfDrawCall)).ConfigureAwait(false); // Wait to enforce the _minimumTimeBetweenStartOfDrawCall

            if (Map is Map map)
            {
                if (map.UpdateAnimations() == true) // Are there animations running on the Map
                    _needsRefresh = true;

                if (map.Navigator.UpdateAnimations()) // Are there animations running on the Navigator
                    _needsRefresh = true;
            }

            if (_needsRefresh)
            {
                _invalidate?.Invoke();
                _needsRefresh = false;
            }
            else
                signalThatDrawingIsDone.Set(); // No new Draw is started so the loop should not wait for it to end.
        }
    }

    private protected void SharedDraw(object canvas)
    {
        if (Renderer is null) 
            return;
        if (Map is null) 
            return;
        if (!Map.Navigator.Viewport.HasSize()) 
            return;

        if (_firstDraw)
        {
            _firstDraw = false;
            Logger.Log(LogLevel.Information, $"First call to the Mapsui renderer");
        }

        // Start stopwatch before updating animations and drawing control
        _stopwatch.Restart();
        _timestampStartDraw = Environment.TickCount;
        // Fetch the image data for all image sources and call RefreshGraphics if new images were loaded.
        _renderer.ImageSourceCache.FetchAllImageData(Mapsui.Styles.Image.SourceToSourceId, Map.FetchMachine, RefreshGraphics);
        
        Renderer.Render(canvas, Map.Navigator.Viewport, Map.Layers, Map.Widgets, Map.BackColor);

        signalThatDrawingIsDone.Set();
        _stopwatch.Stop();

        // If we are interested in performance measurements, we save the new drawing time
        Map.Performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);
    }

    private static int GetAdditionalTimeToDelay(int timestampStartDraw, int minimumTimeBetweenStartOfDrawCall)
    {
        var timeSinceLastDraw = Environment.TickCount - timestampStartDraw;
        var additionalTimeToDelay = Math.Max(minimumTimeBetweenStartOfDrawCall - timeSinceLastDraw, 0);
        return additionalTimeToDelay;
    }

    private void SharedOnSizeChanged(double width, double height)
    {
        _sharedWidth = width;
        _sharedHeight = height;
        TryUpdateViewportSize();
    }

    /// <summary>
    /// Force a update of control
    /// </summary>
    /// <remarks>
    /// When this function is called, the control draws itself once 
    /// </remarks>
    public void ForceUpdate()
    {
        _invalidate?.Invoke();
    }

    /// <summary>
    /// Called whenever a property is changed
    /// </summary>
#if __MAUI__ || __AVALONIA__
    public new event PropertyChangedEventHandler? PropertyChanged;
#else
    public event PropertyChangedEventHandler? PropertyChanged;
#endif

#if __MAUI__
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
#else
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
#endif

    /// <summary>
    /// Unsubscribe from map events 
    /// </summary>
    public void Unsubscribe()
    {
        UnsubscribeFromMapEvents(Map);
    }

    /// <summary>
    /// Subscribe to map events
    /// </summary>
    /// <param name="map">Map, to which events to subscribe</param>
    private void SubscribeToMapEvents(Map map)
    {
        map.DataChanged += Map_DataChanged;
        map.PropertyChanged += Map_PropertyChanged;
        map.RefreshGraphicsRequest += Map_RefreshGraphicsRequest;
    }


    private void Map_RefreshGraphicsRequest(object? sender, EventArgs e)
    {
        RefreshGraphics();
    }

    /// <summary>
    /// Unsubscribe from map events
    /// </summary>
    /// <param name="map">Map, to which events to unsubscribe</param>
    private void UnsubscribeFromMapEvents(Map map)
    {
        var localMap = map;
        localMap.DataChanged -= Map_DataChanged;
        localMap.PropertyChanged -= Map_PropertyChanged;
        localMap.RefreshGraphicsRequest -= Map_RefreshGraphicsRequest;
        localMap.AbortFetch();
    }

    public void Refresh(ChangeType changeType = ChangeType.Discrete)
    {
        Map.Refresh(changeType);
    }

    public void RefreshGraphics()
    {
        _needsRefresh = true;
    }

    private void Map_DataChanged(object? sender, DataChangedEventArgs? e)
    {
        try
        {
            if (e == null)
            {
                Logger.Log(LogLevel.Warning, "Unexpected error: DataChangedEventArgs can not be null");
            }
            else if (e.Error is WebException)
            {
                Logger.Log(LogLevel.Warning, $"A WebException occurred. Do you have internet? Exception: {e.Error?.Message}", e.Error);
            }
            else if (e.Error != null)
            {
                Logger.Log(LogLevel.Warning, $"An error occurred while fetching data. Exception: {e.Error?.Message}", e.Error);
            }
            else // no problems
            {
                RefreshGraphics();
            }
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Warning, $"Unexpected exception in {nameof(Map_DataChanged)}", exception);
        }
    }

    private void Map_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Layers.Layer.Enabled))
        {
            RefreshGraphics();
        }
        else if (e.PropertyName == nameof(Layers.Layer.Opacity))
        {
            RefreshGraphics();
        }
        else if (e.PropertyName == nameof(Map.BackColor))
        {
            RefreshGraphics();
        }
        else if (e.PropertyName == nameof(Layers.Layer.DataSource))
        {
            Refresh(); // There is a new DataSource so let's fetch the new data.
        }
        else if (e.PropertyName == nameof(Map.Extent))
        {
            Refresh();
        }
        else if (e.PropertyName == nameof(Map.Layers))
        {
            Refresh();
        }
    }

    // ReSharper restore RedundantNameQualifier
#pragma warning disable IDISP002 // Is Disposed in SharedDispose
    private DisposableWrapper<Map>? _map;
#pragma warning restore IDISP002

#if __MAUI__

    public static readonly BindableProperty MapProperty = BindableProperty.Create(nameof(Map),
        typeof(Map), typeof(MapControl), default(Map), defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: MapPropertyChanged, propertyChanging: MapPropertyChanging);

    private static void MapPropertyChanging(BindableObject bindable,
        object oldValue, object newValue)
    {
        var mapControl = (MapControl)bindable;
        mapControl.BeforeSetMap();
    }

    private static void MapPropertyChanged(BindableObject bindable,
        object oldValue, object newValue)
    {
        var mapControl = (MapControl)bindable;
        mapControl.AfterSetMap((Map)newValue);
    }

    public Map Map
    {
        get => (Map)GetValue(MapProperty);
        set => SetValue(MapProperty, value);
    }

#else
    /// <summary>
    /// Map holding data for which is shown in this MapControl
    /// </summary>
#if __BLAZOR__
    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
#endif    
#if __WINDOWSFORMS__
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
#endif
    public Map Map
    {
        get
        {
            if (_map == null)
            {
                _map = new DisposableWrapper<Map>(new Map(), true);
                AfterSetMap(_map.WrappedObject);
                OnPropertyChanged();
            }

            return _map.WrappedObject;
        }
        set
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            BeforeSetMap();
            _map?.Dispose();
            _map = new DisposableWrapper<Map>(value, false);
            AfterSetMap(value);
            OnPropertyChanged();
        }
    }
#endif

    private void BeforeSetMap()
    {
        if (Map is null) return; // Although the Map property can not null the map argument can null during initializing and binding.

        UnsubscribeFromMapEvents(Map);
    }

    private void AfterSetMap(Map? map)
    {
        if (map is null)
            return; // Although the Map property can not null the map argument can null during initializing and binding.
        TryUpdateViewportSize();
        SubscribeToMapEvents(map);
        Refresh();
    }

    /// <summary>
    /// Refresh data of Map, but don't paint it
    /// </summary>
    public void RefreshData(ChangeType changeType = ChangeType.Discrete)
    {
        Map.RefreshData(changeType);
    }

    protected void OnMapInfo(MapInfoEventArgs mapInfoEventArgs)
    {
        Map?.OnMapInfo(mapInfoEventArgs); // Also propagate to Map
        Info?.Invoke(this, mapInfoEventArgs);
    }

    /// <inheritdoc />
    public byte[] GetSnapshot(IEnumerable<ILayer>? layers = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
    {
        if (GetPixelDensity() is not float pixelDensity)
            throw new Exception("PixelDensity is not initialized");

        using var stream = Renderer.RenderToBitmapStream(Map.Navigator.Viewport, layers ?? Map?.Layers ?? [], pixelDensity: pixelDensity, renderFormat: renderFormat, quality: quality);
        return stream.ToArray();
    }

    private MapInfoEventArgs CreateMapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType)
    {
        return new MapInfoEventArgs(screenPosition, worldPosition, gestureType, Map, GetMapInfo, GetRemoteMapInfoAsync);
    }

    public MapInfo GetMapInfo(ScreenPosition screenPosition, IEnumerable<ILayer> layers)
    {
        return Renderer.GetMapInfo(screenPosition, Map.Navigator.Viewport, layers);
    }

    protected Task<MapInfo> GetRemoteMapInfoAsync(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers)
    {
        return RemoteMapInfoFetcher.GetRemoteMapInfoAsync(screenPosition, viewport, layers);
    }

    /// <summary>
    /// Tries to set the size of the MapControl.Map.Viewport.
    /// </summary>
    private void TryUpdateViewportSize()
    {
        if (_sharedWidth <= 0 || _sharedHeight <= 0)
            return;

        if (Map is Map map)
        {
            var hadSize = map.Navigator.Viewport.HasSize();
            map.Navigator.SetSize(_sharedWidth, _sharedHeight);
            if (!hadSize && map.Navigator.Viewport.HasSize()) map.OnViewportSizeInitialized();
            Refresh();
        }
    }

    private void SharedDispose(bool disposing)
    {
        if (disposing)
        {
            _isRunning = false;
            Unsubscribe();
            _renderer.Dispose();
            _map?.Dispose();
            _map = null;
        }
    }

    private bool OnWidgetTapped(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType, bool shiftPressed)
    {
        var eventArgs = new WidgetEventArgs(screenPosition, worldPosition, gestureType, Map, shiftPressed, GetMapInfo, GetRemoteMapInfoAsync);

        var touchedWidgets = WidgetInput.GetWidgetsAtPosition(screenPosition, Map);
        foreach (var widget in touchedWidgets)
        {
            if (Logger.Settings.LogWidgetEvents)
                Logger.Log(LogLevel.Information, $"{nameof(OnWidgetTapped)} - {widget.GetType().Name} {nameof(GestureType)}: {gestureType} KeyState: {shiftPressed}");
            widget.OnTapped(eventArgs);
            if (eventArgs.Handled)
                return true;
        }
        return false;
    }

    private bool OnWidgetPointerPressed(ScreenPosition screenPosition, MPoint worldPosition, bool shiftPressed)
    {
        var eventArgs = new WidgetEventArgs(screenPosition, worldPosition, GestureType.Press, Map, shiftPressed, GetMapInfo, GetRemoteMapInfoAsync);
        
        foreach (var widget in WidgetInput.GetWidgetsAtPosition(screenPosition, Map))
        {
            if (Logger.Settings.LogWidgetEvents)
                Logger.Log(LogLevel.Information, $"{nameof(OnWidgetPointerPressed)} - {widget.GetType().Name}");
            widget.OnPointerPressed(eventArgs);
            if (eventArgs.Handled)
                return true;
        }
        return false;
    }

    private bool OnWidgetPointerMoved(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType, bool shiftPressed)
    {
        var eventArgs = new WidgetEventArgs(screenPosition, worldPosition, gestureType, Map, shiftPressed, GetMapInfo, GetRemoteMapInfoAsync);

        foreach (var widget in WidgetInput.GetWidgetsAtPosition(screenPosition, Map))
        {
            widget.OnPointerMoved(eventArgs);
            if (eventArgs.Handled)
                return true;
        }
        return false;
    }

    private bool OnWidgetPointerReleased(ScreenPosition screenPosition, MPoint worldPosition, bool shiftPressed)
    {
        var eventArgs = new WidgetEventArgs(screenPosition, worldPosition, GestureType.Release, Map, shiftPressed, GetMapInfo, GetRemoteMapInfoAsync);
        
        foreach (var widget in WidgetInput.GetWidgetsAtPosition(screenPosition, Map))
        {
            if (Logger.Settings.LogWidgetEvents)
                Logger.Log(LogLevel.Information, $"{nameof(OnWidgetPointerReleased)} - {widget.GetType().Name}");
            widget.OnPointerReleased(eventArgs);
            if (eventArgs.Handled)
                return true;
        }
        return false;
    }

    private bool OnTapped(ScreenPosition screenPosition, GestureType gestureType)
    {
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(screenPosition);
        if (OnWidgetTapped(screenPosition, worldPosition, gestureType, GetShiftPressed()))
            return true;
        if (Map is null)
            return false;
        if (OnMapTapped(screenPosition, worldPosition, gestureType))
            return true;
        OnMapInfo(CreateMapInfoEventArgs(screenPosition, worldPosition, gestureType));
        return false;
    }

    private bool OnPointerPressed(ReadOnlySpan<ScreenPosition> positions)
    {
        if (positions.Length != 1)
            return false;

        _flingTracker.Restart();
        _tapGestureTracker.Restart(positions[0]);
        var screenPosition = positions[0];
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(screenPosition);
        if (OnWidgetPointerPressed(screenPosition, worldPosition, GetShiftPressed()))
            return true;
        return OnMapPointerPressed(screenPosition, worldPosition);
    }

    private bool OnPointerMoved(ReadOnlySpan<ScreenPosition> screenPositions, bool isHovering)
    {
        if (screenPositions.Length != 1)
            return false;

        var gestureType = isHovering ? GestureType.Hover : GestureType.Drag;
        var screenPosition = screenPositions[0];
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(screenPosition);
        if (OnWidgetPointerMoved(screenPosition, worldPosition, gestureType, GetShiftPressed()))
            return true;
        if (OnMapPointerMoved(screenPosition, worldPosition, gestureType))
            return true;
        if (!isHovering)
            _flingTracker.AddEvent(screenPosition, DateTime.Now.Ticks);
        return false;
    }

    private bool OnPointerReleased(ReadOnlySpan<ScreenPosition> screenPositions)
    {
        if (screenPositions.Length != 1)
            return false;
        if (GetPixelDensity() is not float pixelDensity)
            return false;

        var handled = false;
        var screenPosition = screenPositions[0];
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(screenPosition);
        if (OnWidgetPointerReleased(screenPosition, worldPosition, GetShiftPressed()))
            handled = true; // Set to handled but still handle tap in the next line
        if (!handled && OnMapPointerReleased(screenPosition, worldPosition))
            handled = true;
        if (_tapGestureTracker.TapIfNeeded(screenPositions[0], MaxTapGestureMovement * pixelDensity, OnTapped))
            handled = true;
        if (UseFling)
            _flingTracker.FlingIfNeeded((vX, vY) => Map.Navigator.Fling(vX, vY, 1000));
        Refresh();
        return handled;
    }

    protected virtual bool OnMapTapped(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType)
    {
        if (Logger.Settings.LogMapEvents)
            Logger.Log(LogLevel.Information, $"{nameof(OnMapTapped)} - {nameof(GestureType)}: {gestureType}");

        var eventArgs = new MapEventArgs(screenPosition, worldPosition, gestureType, Map, GetMapInfo,
            GetRemoteMapInfoAsync);
        Map.OnTapped(eventArgs);
        if (!eventArgs.Handled)
            MapTapped?.Invoke(this, eventArgs);

        return eventArgs.Handled;
    }

    protected virtual bool OnMapPointerPressed(ScreenPosition screenPosition, MPoint worldPosition)
    {
        if (Logger.Settings.LogMapEvents)
            Logger.Log(LogLevel.Information, $"{nameof(OnMapPointerPressed)}");

        var eventArgs = new MapEventArgs(screenPosition, worldPosition, GestureType.Press, Map, GetMapInfo, 
            GetRemoteMapInfoAsync);
        Map.OnPointerPressed(eventArgs);
        if (!eventArgs.Handled)
            MapPointerPressed?.Invoke(this, eventArgs);

        return eventArgs.Handled;
    }

    protected virtual bool OnMapPointerMoved(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType)
    {
        var eventArgs = new MapEventArgs(screenPosition, worldPosition, gestureType,
            Map, GetMapInfo, GetRemoteMapInfoAsync);
        Map.OnPointerMoved(eventArgs);
        if (!eventArgs.Handled)
            MapPointerMoved?.Invoke(this, eventArgs);

        return eventArgs.Handled;
    }

    protected virtual bool OnMapPointerReleased(ScreenPosition screenPosition, MPoint worldPosition)
    {
        if (Logger.Settings.LogMapEvents)
            Logger.Log(LogLevel.Information, $"{nameof(OnMapPointerReleased)}");

        var eventArgs = new MapEventArgs(screenPosition, worldPosition, GestureType.Release, Map, GetMapInfo, 
            GetRemoteMapInfoAsync);
        Map.OnPointerReleased(eventArgs);
        if (!eventArgs.Handled)
            MapPointerReleased?.Invoke(this, eventArgs);

        return eventArgs.Handled;
    }
}
