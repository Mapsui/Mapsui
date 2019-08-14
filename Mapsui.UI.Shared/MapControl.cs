using Mapsui.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Widgets;
using System.Runtime.CompilerServices;

#if __ANDROID__
namespace Mapsui.UI.Android
#elif __IOS__
namespace Mapsui.UI.iOS
#elif __UWP__
namespace Mapsui.UI.Uwp
#elif __FORMS__
namespace Mapsui.UI.Forms
#else
namespace Mapsui.UI.Wpf
#endif
{
    public partial class MapControl : INotifyPropertyChanged
    {
        private Map _map;
        private double _unSnapRotationDegrees;

        /// <summary>
        /// After how many degrees start rotation to take place
        /// </summary>
        public double UnSnapRotationDegrees
        {
            get { return _unSnapRotationDegrees; }
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
            get { return _reSnapRotationDegrees; }
            set
            {
                if (_reSnapRotationDegrees != value)
                {
                    _reSnapRotationDegrees = value;
                    OnPropertyChanged();
                }
            }
        }

        private IRenderer _renderer = new MapRenderer();

        /// <summary>
        /// Renderer that is used from this MapControl
        /// </summary>
        public IRenderer Renderer
        {
            get { return _renderer; }
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
        private INavigator _navigator;

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
            private set
            {
                _navigator = value ?? throw new ArgumentException($"{nameof(Navigator)} can not be null");
                _navigator.Navigated += Navigated;
            }
        }

        private void Navigated(object sender, EventArgs e)
        {
            _map.Initialized = true;
            Refresh();
        }

        /// <summary>
        /// Called when the viewport is initialized
        /// </summary>
        public event EventHandler ViewportInitialized; //todo: Consider to use the Viewport PropertyChanged

        /// <summary>
        /// Called whenever a feature in one of the layers in InfoLayers is hitten by a click 
        /// </summary>
        public event EventHandler<MapInfoEventArgs> Info;

        /// <summary>
        /// Called whenever a property is changed
        /// </summary>
#if __FORMS__
        public new event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#else
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// Unsubcribe from map events
        /// </summary>
        /// <param name="map">Map, to which events to unsubscribe</param>
        private void UnsubscribeFromMapEvents(Map map)
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
        public void Refresh()
        {
            RefreshData();
            RefreshGraphics();
        }

        private void MapDataChanged(object sender, DataChangedEventArgs e)
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

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            else if (e.PropertyName == nameof(Map.Envelope))
            {
                CallHomeIfNeeded();
                Refresh(); 
            }
            else if (e.PropertyName == nameof(Map.Layers))
            {
                CallHomeIfNeeded();
                Refresh();
            }
            if (e.PropertyName.Equals(nameof(Map.Limiter)))
            {
                _viewport.Limiter = Map.Limiter;
            }
        }
        // ReSharper restore RedundantNameQualifier

        public void CallHomeIfNeeded()
        {
            if (_map != null && !_map.Initialized && _viewport.HasSize && _map?.Envelope != null)
            {
                _map.Home?.Invoke(Navigator);
                _map.Initialized = true;
            }
        }

        /// <summary>
        /// Map holding data for which is shown in this MapControl
        /// </summary>
        public Map Map
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
                    _viewport.Map = Map;
                    _viewport.Limiter = Map.Limiter;
                    CallHomeIfNeeded();
                }

                Refresh();
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public Point ToPixels(Point coordinateInDeviceIndependentUnits)
        {
            return new Point(
                coordinateInDeviceIndependentUnits.X * PixelDensity,
                coordinateInDeviceIndependentUnits.Y * PixelDensity);
        }

        /// <inheritdoc />
        public Point ToDeviceIndependentUnits(Point coordinateInPixels)
        {
            return new Point(coordinateInPixels.X / PixelDensity, coordinateInPixels.Y / PixelDensity);
        }

        private void OnViewportSizeInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Refresh data of Map, but don't paint it
        /// </summary>
        public void RefreshData()
        {
            _map?.RefreshData(Viewport.Extent, Viewport.Resolution, true);
        }

        private void OnInfo(MapInfoEventArgs mapInfoEventArgs)
        {
            if (mapInfoEventArgs == null) return;

            Info?.Invoke(this, mapInfoEventArgs);
        }

        private bool WidgetTouched(IWidget widget, Point screenPosition)
        {
            var result = widget.HandleWidgetTouched(Navigator, screenPosition);

            if (!result && widget is Hyperlink hyperlink && !string.IsNullOrWhiteSpace(hyperlink.Url))
            {
                OpenBrowser(hyperlink.Url);

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(Point screenPosition, int margin = 0)
        {
            return MapInfoHelper.GetMapInfo(Map.Layers.Where(l => l.IsMapInfoLayer).ToList(), Viewport,
                screenPosition, Renderer.SymbolCache, margin);
        }

        /// <summary>
        /// Check if a widget or feature at a given screen position is clicked/tapped
        /// </summary>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
        /// <param name="numTaps">Number of clickes/taps</param>
        /// <returns>True, if something done </returns>
        private MapInfoEventArgs InvokeInfo(Point screenPosition, Point startScreenPosition, int numTaps)
        {
            return InvokeInfo(
                Map.Layers.Where(l => l.IsMapInfoLayer).ToList(), 
                Map.GetWidgetsOfMapAndLayers(), 
                Viewport,
                screenPosition, 
                startScreenPosition, 
                _renderer.SymbolCache, 
                WidgetTouched, 
                numTaps);
        }

        /// <summary>
        /// Check if a widget or feature at a given screen position is clicked/tapped
        /// </summary>
        /// <param name="layers">The layers to query for MapInfo</param>
        /// <param name="widgets">The Map widgets</param>
        /// <param name="viewport">The current Viewport</param>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
        /// <param name="symbolCache">Cache for symbols to determine size</param>
        /// <param name="widgetCallback">Callback, which is called when Widget is hit</param>
        /// <param name="numTaps">Number of clickes/taps</param>
        /// <returns>True, if something done </returns>
        private static MapInfoEventArgs InvokeInfo(IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, 
            IReadOnlyViewport viewport, Point screenPosition, Point startScreenPosition, ISymbolCache symbolCache,
            Func<IWidget, Point, bool> widgetCallback, int numTaps)
        {
            var layerWidgets = layers.Select(l => l.Attribution).Where(a => a != null);
            var allWidgets = layerWidgets.Concat(widgets).ToList(); // Concat layer widgets and map widgets.

            // First check if a Widget is clicked. In the current design they are always on top of the map.
            var touchedWidgets = WidgetTouch.GetTouchedWidget(screenPosition, startScreenPosition, allWidgets);

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
        
            var mapInfo = MapInfoHelper.GetMapInfo(layers, viewport, screenPosition, symbolCache);

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
    }
}

