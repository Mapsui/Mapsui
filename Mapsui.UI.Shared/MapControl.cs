using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

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
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.UI.Blazor;
#else
namespace Mapsui.UI.Wpf;
#endif

public partial class MapControl : INotifyPropertyChanged, IDisposable
{
    private double _unSnapRotationDegrees;
    // Flag indicating if a drawing process is running
    private bool _drawing;
    // Flag indicating if the control has to be redrawn
    private bool _invalidated;
    // Flag indicating if a new drawing process should start
    private bool _refresh;
    // Action to call for a redraw of the control
    private protected Action? _invalidate;
    // Timer for loop to invalidating the control
    private Timer? _invalidateTimer;
    // Interval between two calls of the invalidate function in ms
    private int _updateInterval = 16;
    // Stopwatch for measuring drawing times
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
    // old widget Collection to compare if widget Collection was changed.
    private ConcurrentQueue<IWidget>? _widgetCollection;
    // saving list of touchable Widgets
    private List<ITouchableWidget>? _touchableWidgets;
    // keeps track of the widgets count to see if i need to recalculate the extended widgets.
    private int _updateWidget = 0;
    // keeps track of the widgets count to see if i need to recalculate the touchable widgets.
    private int _updateTouchableWidget;

    private void CommonInitialize()
    {
        // Create map
        Map = new Map();
        // Create timer for invalidating the control
        _invalidateTimer?.Dispose();
        _invalidateTimer = new Timer(InvalidateTimerCallback, null, Timeout.Infinite, 16);
        // Start the invalidation timer
        StartUpdates(false);
    }

    private protected void CommonDrawControl(object canvas)
    {
        if (_drawing) return;
        if (Renderer is null) return;
        if (Map is null) return;
        if (!Map.Navigator.Viewport.HasSize()) return;

        // Start drawing
        _drawing = true;

        // Start stopwatch before updating animations and drawing control
        _stopwatch.Restart();

        // All requested updates up to this point will be handled by this redraw
        _refresh = false;
        Renderer.Render(canvas, Map.Navigator.Viewport, Map.Layers, Map.Widgets, Map.BackColor);

        // Stop stopwatch after drawing control
        _stopwatch.Stop();

        // If we are interested in performance measurements, we save the new drawing time
        _performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);

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
            if (Map is null) return;

            // Check, if we have to redraw the screen

            if (Map?.UpdateAnimations() == true)
                _refresh = true;

            // seems that this could be null sometimes
            if (Map?.Navigator?.UpdateAnimations() ?? false)
                _refresh = true;

            // Check if widgets need refresh
            if (!_refresh && (Map?.Widgets?.Any(w => w.NeedsRedraw) ?? false))
                _refresh = true;

            if (!_refresh)
                return;

            if (_drawing)
            {
                if (_performance != null)
                    _performance.Dropped++;

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
            Logger.Log(LogLevel.Error, ex.Message, ex);
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

    public float PixelDensity => (float)GetPixelDensity();

    private readonly IRenderer _renderer = new MapRenderer();

    /// <summary>
    /// Renderer that is used from this MapControl
    /// </summary>
    public IRenderer Renderer => _renderer;

    /// <summary>
    /// Called whenever the map is clicked. The MapInfoEventArgs contain the features that were hit in
    /// the layers that have IsMapInfoLayer set to true. 
    /// </summary>
    public event EventHandler<MapInfoEventArgs>? Info;

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

    private Map _map = new();

    /// <summary>
    /// Map holding data for which is shown in this MapControl
    /// </summary>
#if __BLAZOR__
    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
#endif
    public Map Map
    {
        get => _map;
        set
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            BeforeSetMap();
            _map = value;
            AfterSetMap(_map);
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

    private void OnInfo(MapInfoEventArgs? mapInfoEventArgs)
    {
        if (mapInfoEventArgs == null) return;

        Map?.OnInfo(mapInfoEventArgs); // Also propagate to Map
        Info?.Invoke(this, mapInfoEventArgs);
    }

    /// <inheritdoc />
    public MapInfo? GetMapInfo(MPoint? screenPosition, int margin = 0)
    {
        if (screenPosition == null)
            return null;

        return Renderer?.GetMapInfo(screenPosition.X, screenPosition.Y, Map.Navigator.Viewport, Map?.Layers ?? [], margin);
    }

    /// <inheritdoc />
    public byte[] GetSnapshot(IEnumerable<ILayer>? layers = null)
    {
        using var stream = Renderer.RenderToBitmapStream(Map.Navigator.Viewport, layers ?? Map?.Layers ?? [], pixelDensity: PixelDensity);
        return stream.ToArray();
    }

    /// <summary>
    /// Check if a widget or feature at a given screen position is clicked/tapped
    /// </summary>
    /// <param name="screenPosition">Screen position to check for widgets and features</param>
    /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
    /// <param name="numTaps">Number of clicks/taps</param>
    /// <returns>True, if something done </returns>
    private MapInfoEventArgs? CreateMapInfoEventArgs(
        MPoint? screenPosition,
        MPoint? startScreenPosition,
        int numTaps)
    {
        if (screenPosition == null || startScreenPosition == null)
            return null;

        // Check which features in the map were tapped.
        var mapInfo = Renderer?.GetMapInfo(screenPosition.X, screenPosition.Y, Map.Navigator.Viewport, Map?.Layers ?? []);

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
            _renderer.Dispose();
        }
        _invalidateTimer = null;
    }

    private bool HandleWidgetPointerMove(MPoint position, bool leftButton, int clickCount, bool shift)
    {
        var touchableWidgets = GetTouchableWidgets();

        if (touchableWidgets.Count == 0)
            return false;

        var widgetArgs = new WidgetTouchedEventArgs(position, clickCount, leftButton, shift);

        foreach (var widget in touchableWidgets)
        {
            if (widget.HandleWidgetMoving(Map.Navigator, position, widgetArgs))
                return true;
        }

        return false;
    }

    private bool HandleTouchingTouched(MPoint position, MPoint? startPosition, bool leftButton, int clickCount, bool shift)
    {
        bool result = HandleWidgetPointerDown(position, leftButton, clickCount, shift);

        if (HandleWidgetPointerUp(position, startPosition, leftButton, clickCount, shift))
        {
            result = true;
        }

        return result;
    }


    private bool HandleWidgetPointerDown(MPoint position, bool leftButton, int clickCount, bool shift)
    {
        var touchableWidgets = GetTouchableWidgets();

        if (touchableWidgets.Count == 0)
            return false;

        var touchedWidgets = WidgetTouch.GetTouchedWidgets(position, position, touchableWidgets);

        foreach (var widget in touchedWidgets)
        {
            var widgetArgs = new WidgetTouchedEventArgs(position, clickCount, leftButton, shift);
            if (widget.HandleWidgetTouching(Map.Navigator, position, widgetArgs))
                return true;
        }

        return false;
    }

    private bool HandleWidgetPointerUp(MPoint position, MPoint? startPosition, bool leftButton, int clickCount, bool shift)
    {
        if (startPosition is null)
        {
            Logger.Log(LogLevel.Error, $"The '{nameof(startPosition)}' is null on release. This is not expected");
            return false;
        }
        var touchableWidgets = GetTouchableWidgets();

        if (touchableWidgets.Count == 0)
            return false;

        var touchedWidgets = WidgetTouch.GetTouchedWidgets(position, position, touchableWidgets);

        foreach (var widget in touchedWidgets)
        {
            if (widget is HyperlinkWidget hyperlink && !string.IsNullOrWhiteSpace(hyperlink.Url))
            {
                // The HyperLink is a special case because we need platform specific code to open the
                // link in a browswer. If the link is not handled within the widget we handle it
                // here and return true to indicate this is handled.
                OpenBrowser(hyperlink.Url!);
                return true;
            }

            var args = new WidgetTouchedEventArgs(position, clickCount, leftButton, shift);

            if (widget.HandleWidgetTouched(Map.Navigator, position, args))
                return true;
        }

        return false;
    }

    private void AssureWidgets()
    {
        if (_widgetCollection != Map.Widgets)
        {
            // reset widgets
            _touchableWidgets = null;
            _widgetCollection = Map.Widgets;
        }
    }

    private List<ITouchableWidget> GetTouchableWidgets()
    {
        AssureWidgets();
        if (_updateTouchableWidget != Map.Widgets.Count || _touchableWidgets == null)
        {
            _updateTouchableWidget = Map.Widgets.Count;
            _touchableWidgets = [];
            var touchableWidgets = Map.GetWidgetsOfMapAndLayers().ToList();
            foreach (var widget in touchableWidgets)
            {
                if (widget is not ITouchableWidget) continue;

                _touchableWidgets.Add((ITouchableWidget)widget);
            }
        }

        return _touchableWidgets;
    }
}
