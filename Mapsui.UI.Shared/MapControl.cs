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
    // Flag indicating if a drawing process is running
    private bool _drawing;
    // Flag indicating if the control has to be redrawn
    private bool _invalidated;
    // Flag indicating if a new drawing process should start
    private bool _refresh;
    // Action to call for a redraw of the control
    private protected Action? _invalidate;
    // Timer for loop to invalidating the control
    private System.Threading.Timer? _invalidateTimer;
    // Interval between two calls of the invalidate function in ms
    private int _updateInterval = 16;
    // Stopwatch for measuring drawing times
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
#pragma warning disable IDISP002 // Is disposed in CommonDispose
    private readonly IRenderer _renderer = new MapRenderer();
#pragma warning restore IDISP002
    private readonly TapGestureTracker _tapGestureTracker = new();
    private readonly FlingTracker _flingTracker = new();
    private static bool _firstRender = true;

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

    public float PixelDensity => (float)GetPixelDensity();

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
        // Create timer for invalidating the control
        _invalidateTimer?.Dispose();
        _invalidateTimer = new (InvalidateTimerCallback, null, Timeout.Infinite, 16);
        // Start the invalidation timer
        StartUpdates(false);
        // Mapsui.Rendering.Skia use Mapsui.Nts where GetDbaseLanguageDriver need encoding providers
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    private protected void CommonDrawControl(object canvas)
    {
        if (_drawing) return;
        if (Renderer is null) return;
        if (Map is null) return;
        if (!Map.Navigator.Viewport.HasSize()) return;

        if (_firstRender)
        {
            _firstRender = false;
            Logger.Log(LogLevel.Information, $"First call to the Mapsui renderer");
        }

        // Start drawing
        _drawing = true;

        // Start stopwatch before updating animations and drawing control
        _stopwatch.Restart();

        // All requested updates up to this point will be handled by this redraw
        _refresh = false;

        // Fetch the image data for all image sources and call RefreshGraphics if new images were loaded.
        _renderer.ImageSourceCache.FetchAllImageData(Mapsui.Styles.Image.SourceToSourceId, Map.FetchMachine, RefreshGraphics);
        
        Renderer.Render(canvas, Map.Navigator.Viewport, Map.Layers, Map.Widgets, Map.BackColor);

        // Stop stopwatch after drawing control
        _stopwatch.Stop();

        // If we are interested in performance measurements, we save the new drawing time
        Map.Performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);

        // End drawing
        _drawing = false;
        _invalidated = false;
    }

    private void InvalidateTimerCallback(object? state)
    {
        try
        {
            // In MAUI if you use binding there is an event where the new value is null even though
            // the current value en the value you are binding to are not null. Perhaps this should be
            // considered a bug.
            if (Map is null) 
                return;

            // Check, if we have to redraw the screen

            if (Map.UpdateAnimations() == true)
                _refresh = true;

            // seems that this could be null sometimes
            if (Map.Navigator?.UpdateAnimations() ?? false)
                _refresh = true;

            // Check if widgets need refresh
            if (!_refresh && (Map.Widgets?.Any(w => w.NeedsRedraw) ?? false))
                _refresh = true;

            if (!_refresh)
                return;

            if (_drawing)
            {
                Map.Performance.Dropped++;
                return;
            }

            if (_invalidated)
            {
                return;
            }

            _invalidated = true;
            _invalidate?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error in render loop", ex);
        }
    }

    /// <summary>
    /// Start updates for control
    /// </summary>
    /// <remarks>
    /// When this function is called, the control is redrawn if needed
    /// </remarks>
    public void StartUpdates(bool refresh = true)
    {
        _refresh = refresh;
        _invalidateTimer?.Change(0, _updateInterval);
    }

    /// <summary>
    /// Stop updates for control
    /// </summary>
    /// <remarks>
    /// When this function is called, the control stops to redraw itself, 
    /// even if it is needed
    /// </remarks>
    public void StopUpdates()
    {
        _invalidateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Force a update of control
    /// </summary>
    /// <remarks>
    /// When this function is called, the control draws itself once 
    /// </remarks>
    public void ForceUpdate()
    {
        _invalidated = true;
        _invalidate?.Invoke();
    }

    /// <summary>
    /// Interval between two redraws of the MapControl in ms
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] 
    public int UpdateInterval
    {
        get => _updateInterval;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(UpdateInterval), value, "Parameter must be greater than zero");

            if (_updateInterval != value)
            {
                _updateInterval = value;
                StartUpdates();
            }
        }
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
        _refresh = true;
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
    // ReSharper disable RedundantNameQualifier - needed for iOS for disambiguation

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
#pragma warning disable IDISP002 // Is Disposed in Common Dispose
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
        if (map is null) return; // Although the Map property can not null the map argument can null during initializing and binding.

        if (HasSize())
            map.Navigator.SetSize(ViewportWidth, ViewportHeight);
        SubscribeToMapEvents(map);
        Refresh();
    }

    /// <inheritdoc />
    public MPoint ToPixels(MPoint coordinateInDeviceIndependentUnits)
    {
        return new MPoint(
            coordinateInDeviceIndependentUnits.X * PixelDensity,
            coordinateInDeviceIndependentUnits.Y * PixelDensity);
    }

    /// <inheritdoc />
    public MPoint ToDeviceIndependentUnits(MPoint coordinateInPixels)
    {
        return new MPoint(coordinateInPixels.X / PixelDensity, coordinateInPixels.Y / PixelDensity);
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
        using var stream = Renderer.RenderToBitmapStream(Map.Navigator.Viewport, layers ?? Map?.Layers ?? [], pixelDensity: PixelDensity, renderFormat: renderFormat, quality: quality);
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

    private void SetViewportSize()
    {
        var hadSize = Map.Navigator.Viewport.HasSize();
        Map.Navigator.SetSize(ViewportWidth, ViewportHeight);
        if (!hadSize && Map.Navigator.Viewport.HasSize()) Map.OnViewportSizeInitialized();
        Refresh();
    }

    private void CommonDispose(bool disposing)
    {
        if (disposing)
        {
            Unsubscribe();
            StopUpdates();
            _invalidateTimer?.Dispose();
            _invalidateTimer = null;
            _renderer.Dispose();
            _map?.Dispose();
            _map = null;
        }
        _invalidateTimer = null;
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

        var handled = false;
        var screenPosition = screenPositions[0];
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(screenPosition);
        if (OnWidgetPointerReleased(screenPosition, worldPosition, GetShiftPressed()))
            handled = true; // Set to handled but still handle tap in the next line
        if (!handled && OnMapPointerReleased(screenPosition, worldPosition))
            handled = true;
        if (_tapGestureTracker.TapIfNeeded(screenPositions[0], MaxTapGestureMovement * PixelDensity, OnTapped))
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

    private bool HasSize() => ViewportWidth > 0 && ViewportHeight > 0;
}
