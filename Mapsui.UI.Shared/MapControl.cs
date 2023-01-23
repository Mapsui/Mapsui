using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
using Mapsui.Widgets;

#nullable enable
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables

#if __MAUI__
using Microsoft.Maui.Controls;
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
namespace Mapsui.UI.Blazor;
#else
namespace Mapsui.UI.Wpf;
#endif

public partial class MapControl : INotifyPropertyChanged, IDisposable
{
    private double _unSnapRotationDegrees;
    // Flag indicating if a drawing process is running
    private bool _drawing;
    // Flag indicating if a new drawing process should start
    private bool _refresh;
    // Action to call for a redraw of the control
    private protected Action? _invalidate;
    // Timer for loop to invalidating the control
    private System.Threading.Timer? _invalidateTimer;
    // Interval between two calls of the invalidate function in ms
    private int _updateInterval = 16;
    // Stopwatch for measuring drawing times
    private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

    private protected void CommonInitialize()
    {
        // Create map
        Map = new Map();
        // Create timer for invalidating the control
        _invalidateTimer?.Dispose();
        _invalidateTimer = new System.Threading.Timer(InvalidateTimerCallback, null, System.Threading.Timeout.Infinite, 16);
        // Start the invalidation timer
        StartUpdates(false);
    }

    private protected void CommonDrawControl(object canvas)
    {
        if (_drawing)
            return;
        if (Renderer == null)
            return;
        if (Map == null)
            return;
        if (!Viewport.HasSize())
            return;

        // Start drawing
        _drawing = true;

        // Start stopwatch before updating animations and drawing control
        _stopwatch.Restart();

        // All requested updates up to this point will be handled by this redraw
        _refresh = false;
        Renderer.Render(canvas, new Viewport(Viewport), Map.Layers, Map.Widgets, Map.BackColor);

        // Stop stopwatch after drawing control
        _stopwatch.Stop();

        // If we are interested in performance measurements, we save the new drawing time
        _performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);

        // End drawing
        _drawing = false;
    }

    private void InvalidateTimerCallback(object? state)
    {
        // Check, if we have to redraw the screen

        if (Map?.UpdateAnimations() == true)
            _refresh = true;

        if (_viewport.UpdateAnimations())
            _refresh = true;

        if (!_refresh)
            return;

        if (_drawing)
        {
            if (_performance != null)
                _performance.Dropped++;

            return;
        }

        _invalidate?.Invoke();
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
        _invalidateTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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
    /// Interval between two redraws of the MapControl in ms
    /// </summary>
    public int UpdateInterval
    {
        get => _updateInterval;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(UpdateInterval)} must be greater than 0");

            if (_updateInterval != value)
            {
                _updateInterval = value;
                StartUpdates();
            }
        }
    }

    private Performance? _performance;

    /// <summary>
    /// Object to save performance information about the drawing of the map
    /// </summary>
    /// <remarks>
    /// If this is null, no performance information is saved.
    /// </remarks>
    public Performance? Performance
    {
        get => _performance;
        set
        {
            if (_performance != value)
            {
                _performance = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// After how many degrees start rotation to take place
    /// </summary>
    public double UnSnapRotationDegrees
    {
        get => _unSnapRotationDegrees;
        set
        {
            if (_unSnapRotationDegrees != value)
            {
                _unSnapRotationDegrees = value;
                OnPropertyChanged();
            }
        }
    }

    private double _reSnapRotationDegrees;

    /// <summary>
    /// With how many degrees from 0 should map snap to 0 degrees
    /// </summary>
    public double ReSnapRotationDegrees
    {
        get => _reSnapRotationDegrees;
        set
        {
            if (_reSnapRotationDegrees != value)
            {
                _reSnapRotationDegrees = value;
                OnPropertyChanged();
            }
        }
    }

    public float PixelDensity => GetPixelDensity();

    private IRenderer _renderer = new MapRenderer();

    /// <summary>
    /// Renderer that is used from this MapControl
    /// </summary>
    public IRenderer Renderer
    {
        get => _renderer;
        set
        {
            if (value is null) throw new NullReferenceException(nameof(Renderer));
            if (_renderer != value)
            {
                _renderer = value;
                OnPropertyChanged();
            }
        }
    }

    private protected readonly LimitedViewport _viewport = new LimitedViewport();
    private INavigator? _navigator;

    /// <summary>
    /// Viewport holding information about visible part of the map. Viewport can never be null.
    /// </summary>
    public IReadOnlyViewport Viewport => _viewport;

    /// <summary>
    /// Handles all manipulations of the map viewport
    /// </summary>
    public INavigator? Navigator
    {
        get => _navigator;
        set
        {
            if (_navigator != null)
            {
                _navigator.Navigated -= Navigated;
            }
            _navigator = value ?? throw new ArgumentException($"{nameof(Navigator)} can not be null");
            _navigator.Navigated += Navigated;
        }
    }

    private void Navigated(object? sender, ChangeType changeType)
    {
        if (Map != null)
        {
            Map.Initialized = true;
        }

        Refresh(changeType);
    }

    /// <summary>
    /// Called when the viewport is initialized
    /// </summary>
    public event EventHandler? ViewportInitialized; //todo: Consider to use the Viewport PropertyChanged

    /// <summary>
    /// Called whenever the map is clicked. The MapInfoEventArgs contain the features that were hit in
    /// the layers that have IsMapInfoLayer set to true. 
    /// </summary>
    public event EventHandler<MapInfoEventArgs>? Info;

    /// <summary>
    /// Called whenever a property is changed
    /// </summary>
#if __FORMS__ || __MAUI__ || __AVALONIA__
    public new event PropertyChangedEventHandler? PropertyChanged;
#else
    public event PropertyChangedEventHandler? PropertyChanged;
#endif

#if __FORMS__ || __MAUI__
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
        map.DataChanged += MapDataChanged;
        map.PropertyChanged += MapPropertyChanged;
    }

    /// <summary>
    /// Unsubscribe from map events
    /// </summary>
    /// <param name="map">Map, to which events to unsubscribe</param>
    private void UnsubscribeFromMapEvents(Map? map)
    {
        var localMap = map;
        if (localMap != null)
        {
            localMap.DataChanged -= MapDataChanged;
            localMap.PropertyChanged -= MapPropertyChanged;
            localMap.AbortFetch();
        }
    }

    /// <summary>
    /// Refresh data of the map and than repaint it
    /// </summary>
    public void Refresh(ChangeType changeType = ChangeType.Discrete)
    {
        RefreshData(changeType);
        RefreshGraphics();
    }

    public void RefreshGraphics()
    {
        _refresh = true;
    }

    private void MapDataChanged(object? sender, DataChangedEventArgs? e)
    {
        RunOnUIThread(() =>
        {
            try
            {
                if (e == null)
                {
                    Logger.Log(LogLevel.Warning, "Unexpected error: DataChangedEventArgs can not be null");
                }
                else if (e.Cancelled)
                {
                    Logger.Log(LogLevel.Warning, "Fetching data was cancelled.");
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
                Logger.Log(LogLevel.Warning, $"Unexpected exception in {nameof(MapDataChanged)}", exception);
            }
        });
    }
    // ReSharper disable RedundantNameQualifier - needed for iOS for disambiguation

    private void MapPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
            CallHomeIfNeeded();
            Refresh();
        }
        else if (e.PropertyName == nameof(Map.Layers))
        {
            CallHomeIfNeeded();
            Refresh();
        }
        if (e.PropertyName == nameof(Map.Limiter))
        {
            _viewport.Limiter = Map?.Limiter;
        }
    }
    // ReSharper restore RedundantNameQualifier

    public void CallHomeIfNeeded()
    {
        if (Map != null && !Map.Initialized && _viewport.HasSize() && Map?.Extent != null && Navigator != null)
        {
            Map.Home?.Invoke(Navigator);
            Map.Initialized = true;
        }
    }

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
        mapControl.AfterSetMap(mapControl.Map);
    }


    public Map? Map
    {
        get => (Map)GetValue(MapProperty);
        set => SetValue(MapProperty, value);
    }

#else

    private Map? _map;

    /// <summary>
    /// Map holding data for which is shown in this MapControl
    /// </summary>
#if __BLAZOR__
    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
#endif
    public Map? Map
    {
        get => _map;
        set
        {
            BeforeSetMap();
            _map = value;
            AfterSetMap(_map);
            OnPropertyChanged();
        }
    }
#endif

    private void BeforeSetMap()
    {
        UnsubscribeFromMapEvents(Map);
    }

    private void AfterSetMap(Map? map)
    {
        if (map != null)
        {
            SubscribeToMapEvents(map);
            Navigator = new Navigator(map, _viewport);
            _viewport.Map = map;
            _viewport.Limiter = map.Limiter;
            CallHomeIfNeeded();
        }

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

    private void OnViewportSizeInitialized()
    {
        ViewportInitialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refresh data of Map, but don't paint it
    /// </summary>
    public void RefreshData(ChangeType changeType = ChangeType.Discrete)
    {
        if (Viewport.Extent == null)
            return;
        if (Viewport.Extent.GetArea() <= 0)
            return;

        var fetchInfo = new FetchInfo(Viewport.Extent, Viewport.Resolution, Map?.CRS, changeType);
        Map?.RefreshData(fetchInfo);
    }

    private protected void OnInfo(MapInfoEventArgs? mapInfoEventArgs)
    {
        if (mapInfoEventArgs == null) return;

        Map?.OnInfo(mapInfoEventArgs); // Also propagate to Map
        Info?.Invoke(this, mapInfoEventArgs);
    }

    private bool WidgetTouched(IWidget widget, MPoint screenPosition)
    {
        var result = Navigator != null && widget.HandleWidgetTouched(Navigator, screenPosition);

        if (!result && widget is Hyperlink hyperlink && !string.IsNullOrWhiteSpace(hyperlink.Url))
        {
            OpenBrowser(hyperlink.Url!);
        }

        return result;
    }

    /// <inheritdoc />
    public MapInfo? GetMapInfo(MPoint? screenPosition, int margin = 0)
    {
        if (screenPosition == null)
            return null;

        return Renderer?.GetMapInfo(screenPosition.X, screenPosition.Y, Viewport, Map?.Layers ?? new LayerCollection(), margin);
    }

    /// <inheritdoc />
    public byte[] GetSnapshot(IEnumerable<ILayer>? layers = null)
    {
        using var stream = Renderer.RenderToBitmapStream(Viewport, layers ?? Map?.Layers ?? new LayerCollection(), pixelDensity: PixelDensity);
        return stream.ToArray();
    }

    /// <summary>
    /// Check if a widget or feature at a given screen position is clicked/tapped
    /// </summary>
    /// <param name="screenPosition">Screen position to check for widgets and features</param>
    /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
    /// <param name="numTaps">Number of clickes/taps</param>
    /// <returns>True, if something done </returns>
    private protected MapInfoEventArgs? InvokeInfo(MPoint? screenPosition, MPoint? startScreenPosition, int numTaps)
    {
        return InvokeInfo(
            Map?.GetWidgetsOfMapAndLayers() ?? new List<IWidget>(),
            screenPosition,
            startScreenPosition,
            WidgetTouched,
            numTaps);
    }

    /// <summary>
    /// Check if a widget or feature at a given screen position is clicked/tapped
    /// </summary>
    /// <param name="widgets">The Map widgets</param>
    /// <param name="screenPosition">Screen position to check for widgets and features</param>
    /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
    /// <param name="widgetCallback">Callback, which is called when Widget is hit</param>
    /// <param name="numTaps">Number of clickes/taps</param>
    /// <returns>True, if something done </returns>
    private MapInfoEventArgs? InvokeInfo(IEnumerable<IWidget> widgets, MPoint? screenPosition,
        MPoint? startScreenPosition, Func<IWidget, MPoint, bool> widgetCallback, int numTaps)
    {
        if (screenPosition == null || startScreenPosition == null)
            return null;

        // Check if a Widget is tapped. In the current design they are always on top of the map.
        var touchedWidgets = WidgetTouch.GetTouchedWidget(screenPosition, startScreenPosition, widgets);

        foreach (var widget in touchedWidgets)
        {
            var result = widgetCallback(widget, screenPosition);

            if (result)
            {
                return new MapInfoEventArgs
                {
                    Handled = true
                };
            }
        }

        // Check which features in the map were tapped.
        var mapInfo = Renderer?.GetMapInfo(screenPosition.X, screenPosition.Y, Viewport, Map?.Layers ?? new LayerCollection());

        if (mapInfo != null)
        {
            return new MapInfoEventArgs
            {
                MapInfo = mapInfo,
                NumTaps = numTaps,
                Handled = false
            };
        }

        return null;
    }

    private protected void SetViewportSize()
    {
        var hadSize = Viewport.HasSize();
        _viewport.SetSize(ViewportWidth, ViewportHeight);
        if (!hadSize && Viewport.HasSize()) OnViewportSizeInitialized();
        CallHomeIfNeeded();
        Refresh();
    }

    private protected void CommonDispose(bool disposing)
    {
        if (disposing)
        {
            Unsubscribe();
            StopUpdates();
            _invalidateTimer?.Dispose();
        }
        _invalidateTimer = null;
    }
}
