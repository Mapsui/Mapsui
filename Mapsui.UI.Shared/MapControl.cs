using Mapsui.Geometries;
using System;
using System.ComponentModel;
using System.Net;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;

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

        /// <summary>
        /// Allow map panning through touch or mouse
        /// </summary>
        public bool PanLock { get; set; }

        /// <summary>
        /// Allow a rotation with a pinch gesture
        /// </summary>
        public bool RotationLock { get; set; } = true;

        /// <summary>
        /// Allow zooming though touch or mouse
        /// </summary>
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

        public event EventHandler ViewportInitialized; //todo: Consider to use the Viewport PropertyChanged

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
            map.RefreshGraphics += MapRefreshGraphics;
        }

        private void MapRefreshGraphics(object sender, EventArgs eventArgs)
        {
            RefreshGraphics();
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
                temp.RefreshGraphics -= MapRefreshGraphics;
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
                    _map.RefreshData(true);
                }

                RefreshGraphics();
            }
        }

        /// <summary>
        /// Converts coordinates in device independent units (or DIP or DP) to pixels.
        /// </summary>
        /// <param name="coordinateInDeviceIndependentUnits">Coordinate in device independent units (or DIP or DP)</param>
        /// <returns>Coordinate in pixels</returns>
        public Point ToPixels(Point coordinateInDeviceIndependentUnits)
        {
            return new Point(
                coordinateInDeviceIndependentUnits.X * PixelsPerDeviceIndependentUnit,
                coordinateInDeviceIndependentUnits.Y * PixelsPerDeviceIndependentUnit);
        }

        /// <summary>
        /// Converts coordinates in pixels to device independent units (or DIP or DP).
        /// </summary>
        /// <param name="coordinateInPixels">Coordinate in pixels</param>
        /// <returns>Coordinate in device independent units (or DIP or DP)</returns>
        public Point ToDeviceIndependentUnits(Point coordinateInPixels)
        {
            return new Point(
                coordinateInPixels.X / PixelsPerDeviceIndependentUnit,
                coordinateInPixels.Y / PixelsPerDeviceIndependentUnit);
        }

        private void TryInitializeViewport(double screenWidth, double screenHeight)
        {
            if (_map?.Viewport?.Initialized != false) return;

            if (_map.Viewport.TryInitializeViewport(_map.Envelope, screenWidth, screenHeight))
            {
                // limiter now only properly implemented in WPF.
                ViewportLimiter.Limit(_map.Viewport, _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                    _map.PanMode, _map.PanLimits, _map.Envelope);

                Map.RefreshData(true);
                OnViewportInitialized();
            }
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshData()
        {
            _map?.RefreshData(true);
        }
    }
}
