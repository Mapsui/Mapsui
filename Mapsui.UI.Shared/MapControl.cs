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

#if __ANDROID__
namespace Mapsui.UI.Android
#elif __IOS__
namespace Mapsui.UI.iOS
#elif __UWP__
namespace Mapsui.UI.Uwp
#elif __FORMS__
namespace Mapsui.UI
#else
namespace Mapsui.UI.Wpf
#endif
{
    public partial class MapControl
    {
        private Map _map;

        /// <inheritdoc />
        public bool PanLock { get; set; }

        /// <inheritdoc />
        public bool RotationLock { get; set; } = true;

        /// <inheritdoc />
        public bool ZoomLock { get; set; }

        /// <summary>
        /// After how many degrees start rotation to take place
        /// </summary>
        public double UnSnapRotationDegrees { get; set; }

        /// <summary>
        /// With how many degrees from 0 should map snap to 0 degrees
        /// </summary>
        public double ReSnapRotationDegrees { get; set; }
        
        public IRenderer Renderer { get; set; } = new MapRenderer();
        
        /// <summary>
        /// Viewport holding informations about visible part of the map. Viewport can never be null.
        /// </summary>
        private readonly IViewport _viewport = new Viewport();

        public IReadOnlyViewport Viewport => _viewport;

        public INavigator Navigator { get; private set; }

        public event EventHandler ViewportInitialized; //todo: Consider to use the Viewport PropertyChanged

        /// <summary>
        ///  Called whenever a feature in one of the layers in InfoLayers is hitten by a click 
        /// </summary>
        public event EventHandler<MapInfoEventArgs> Info;

        /// <summary>
        /// Unsubscribe from map events </summary>
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
                RefreshData(); // There is a new datasource so let's fetch the new data.
            }
            else if (e.PropertyName == nameof(Map.Layers))
            {
                Refresh();
            }
        }

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
                    if (_viewport.IsSizeInitialized()) _map.Home(Navigator); // If size is not initialized it will be called at set size. This is okay.
                    RefreshData();
                }

                RefreshGraphics();
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

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshData()
        {
            _map?.RefreshData(Viewport.Extent, Viewport.Resolution, true);
        }

        /// <summary>
        /// Internally we want to call RefreshData with a minor change in some cases.
        /// Users should just always call RefreshData without arguments
        /// </summary>
        /// <param name="majorChange"></param>
        private void RefreshData(bool majorChange)
        {
            _map?.RefreshData(Viewport.Extent, Viewport.Resolution, majorChange);
        }

        private void OnInfo(MapInfoEventArgs mapInfoEventArgs)
        {
            if (mapInfoEventArgs == null) return;

            Info?.Invoke(this, mapInfoEventArgs);
        }

        private void WidgetTouched(IWidget widget, Point screenPosition)
        {
            if (widget is Hyperlink hyperlink)
            {
                OpenBrowser(hyperlink.Url);
            }

            widget.HandleWidgetTouched(Navigator, screenPosition);
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(Point screenPosition, int margin = 0)
        {
            return MapInfoHelper.GetMapInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Viewport,
                screenPosition, Renderer.SymbolCache, margin);
        }

        /// <inheritdoc />
        public MapInfo GetMapInfo(IEnumerable<ILayer> layers, Point screenPosition, int margin = 0)
        {
            return MapInfoHelper.GetMapInfo(layers, Viewport,
                screenPosition, Renderer.SymbolCache, margin);
        }

        /// <summary>
        /// Check, if a widget or feature at a given screen position is clicked/tapped
        /// </summary>
        /// <param name="layers">The layers to query for MapInfo</param>
        /// <param name="widgets">The Map widgets</param>
        /// <param name="viewport">The current Viewport</param>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
        /// <param name="symbolCache">Cache for symbols to determin size</param>
        /// <param name="widgetCallback">Callback, which is called when Widget is hiten</param>
        /// <param name="numTaps">Number of clickes/taps</param>
        /// <returns>True, if something done </returns>
        private static MapInfoEventArgs InvokeInfo(IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, 
            IReadOnlyViewport viewport, Point screenPosition, Point startScreenPosition, ISymbolCache symbolCache,
            Action<IWidget, Point> widgetCallback, int numTaps)
        {
            var layerWidgets = layers.Select(l => l.Attribution).Where(a => a != null);
            var allWidgets = layerWidgets.Concat(widgets).ToList(); // Concat layer widgets and map widgets.

            // First check if a Widget is clicked. In the current design they are always on top of the map.
            var widget = WidgetTouch.GetTouchedWidget(screenPosition, startScreenPosition, allWidgets);
            if (widget != null)
            {
                // todo:
                // How should widgetCallback have a handled type thing?
                // Widgets should be iterated through rather than getting a single widget, 
                // based on Z index and then called until handled = true; Ordered By highest Z
                widgetCallback(widget, screenPosition);
                return null;
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
            var wasSizeInitialized = _viewport.IsSizeInitialized();
            _viewport.SetSize(ViewportWidth, ViewportHeight);

            if (!wasSizeInitialized && _viewport.IsSizeInitialized())
            {
                Map?.Home(Navigator); // When Map is null here Home will be called on Map set. So this is okay.
                OnViewportInitialized();
            }

            Refresh();
        }

        public void Clear()
        {
            // not sure if we need this method
            _map?.ClearCache();
            RefreshGraphics();
        }
    }
}

