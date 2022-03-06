﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
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
namespace Mapsui.UI.Maui
#elif __UWP__
namespace Mapsui.UI.Uwp
#elif __ANDROID__
namespace Mapsui.UI.Android
#elif __IOS__
namespace Mapsui.UI.iOS
#elif __WINUI__
namespace Mapsui.UI.WinUI
#elif __FORMS__
namespace Mapsui.UI.Forms
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto
#else
namespace Mapsui.UI.Wpf
#endif
{
    public partial class MapControl : INotifyPropertyChanged, IDisposable
    {
        private Map? _map;
        private double _unSnapRotationDegrees;
        // Flag indicating if a drawing process is running
        private bool _drawing;
        // Flag indicating if a new drawing process should start
        private bool _refresh;
        // Action to call for a redraw of the control
        private Action? _invalidate;
        // Timer for loop to invalidating the control
        private System.Threading.Timer? _invalidateTimer = default!;
        // Interval between two calls of the invalidate function in ms
        private int _updateInterval = 16;
        // Stopwatch for measuring drawing times
        private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        private void CommonInitialize()
        {
            // Create map
            Map = new Map();
            // Create timer for invalidating the control
            _invalidateTimer?.Dispose();
            _invalidateTimer = new System.Threading.Timer(InvalidateTimerCallback, null, System.Threading.Timeout.Infinite, 16);
            // Start the invalidation timer
            StartUpdates(false);
        }

        private void CommonDrawControl(object canvas)
        {
            if (_drawing)
                return;
            if (Renderer == null)
                return;
            if (_map == null)
                return;
            if (!Viewport.HasSize)
                return;

            // Start drawing
            _drawing = true;

            // Start stopwatch before updating animations and drawing control
            _stopwatch.Restart();

            // All requested updates up to this point will be handled by this redraw
            _refresh = false;
            Renderer.Render(canvas, new Viewport(Viewport), _map.Layers, _map.Widgets, _map.BackColor);

            // Stop stopwatch after drawing control
            _stopwatch.Stop();

            // If we are interested in performance measurements, we save the new drawing time
            _performance?.Add(_stopwatch.Elapsed.TotalMilliseconds);

            // Log drawing time
            Logger.Log(LogLevel.Information, $"Time for drawing control [ms]: {_stopwatch.Elapsed.TotalMilliseconds}");

            // End drawing
            _drawing = false;
        }

        private void InvalidateTimerCallback(object? state)
        {
            // Check, if we have to redraw the screen

            if (_map?.UpdateAnimations() == true)
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
                if (_renderer != value)
                {
                    _renderer = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly LimitedViewport _viewport = new LimitedViewport();
        private INavigator _navigator = default!;

        /// <summary>
        /// Viewport holding information about visible part of the map. Viewport can never be null.
        /// </summary>
        public IReadOnlyViewport Viewport => _viewport;

        /// <summary>
        /// Handles all manipulations of the map viewport
        /// </summary>
        public INavigator Navigator
        {
            get => _navigator;
            set
            {
                if (_navigator != null)
                {
                    _navigator.Navigated -= Navigated;
#pragma warning disable IDISP007 // Don't dispose injected
                    _navigator.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
                }
                _navigator = value ?? throw new ArgumentException($"{nameof(Navigator)} can not be null");
                _navigator.Navigated += Navigated;
            }
        }

        private void Navigated(object? sender, ChangeType changeType)
        {
            if (_map != null)
            {
                _map.Initialized = true;
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
            UnsubscribeFromMapEvents(_map);
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
            var temp = map;
            if (temp != null)
            {
                temp.DataChanged -= MapDataChanged;
                temp.PropertyChanged -= MapPropertyChanged;
                temp.AbortFetch();
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
            RunOnUIThread(() => {
                try
                {
                    if (e == null)
                    {
                        Logger.Log(LogLevel.Warning, "Unexpected error: DataChangedEventArgs can not be null");
                    }
                    else if (e.Cancelled)
                    {
                        Logger.Log(LogLevel.Warning, "Fetching data was cancelled", e.Error);
                    }
                    else if (e.Error is WebException)
                    {
                        Logger.Log(LogLevel.Warning, "A WebException occurred. Do you have internet?", e.Error);
                    }
                    else if (e.Error != null)
                    {
                        Logger.Log(LogLevel.Warning, "An error occurred while fetching data", e.Error);
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
            if (_map != null && !_map.Initialized && _viewport.HasSize && _map?.Extent != null)
            {
                _map.Home?.Invoke(Navigator);
                _map.Initialized = true;
            }
        }

        /// <summary>
        /// Map holding data for which is shown in this MapControl
        /// </summary>
        public Map? Map
        {
            get => _map;
            set
            {
                if (_map != null)
                {
                    UnsubscribeFromMapEvents(_map);
                    _map = null;
                }

                _map = value;

                if (_map != null)
                {
                    SubscribeToMapEvents(_map);
                    Navigator = new Navigator(_map, _viewport);
                    _viewport.Map = _map;
                    _viewport.Limiter = _map.Limiter;
                    CallHomeIfNeeded();
                }

                Refresh();
                OnPropertyChanged();
            }
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
            _map?.RefreshData(fetchInfo);
        }

        private void OnInfo(MapInfoEventArgs? mapInfoEventArgs)
        {
            if (mapInfoEventArgs == null) return;

            Map?.OnInfo(mapInfoEventArgs); // Also propagate to Map
            Info?.Invoke(this, mapInfoEventArgs);
        }

        private bool WidgetTouched(IWidget widget, MPoint screenPosition)
        {
            var result = widget.HandleWidgetTouched(Navigator, screenPosition);

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
        public byte[]? GetSnapshot(IEnumerable<ILayer>? layers = null)
        {
            byte[]? result = null;

            using (var stream = Renderer?.RenderToBitmapStream(Viewport, layers ?? Map?.Layers ?? new LayerCollection(), pixelDensity: PixelDensity))
            {
                if (stream != null)
                    result = stream.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Check if a widget or feature at a given screen position is clicked/tapped
        /// </summary>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
        /// <param name="numTaps">Number of clickes/taps</param>
        /// <returns>True, if something done </returns>
        private MapInfoEventArgs? InvokeInfo(MPoint? screenPosition, MPoint? startScreenPosition, int numTaps)
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

        private void SetViewportSize()
        {
            var hadSize = Viewport.HasSize;
            _viewport.SetSize(ViewportWidth, ViewportHeight);
            if (!hadSize && Viewport.HasSize) OnViewportSizeInitialized();
            CallHomeIfNeeded();
            Refresh();
        }

        /// <summary>
        /// Clear cache and repaint map
        /// </summary>
        public void Clear()
        {
            // not sure if we need this method
            _map?.ClearCache();
            RefreshGraphics();
        }

        private void CommonDispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
#pragma warning disable IDISP007 // Don't dispose injected
                _navigator?.Dispose();
#pragma warning restore IDISP007
                StopUpdates();
                _invalidateTimer?.Dispose();
            }
            _invalidateTimer = null;
        }
    }
}

