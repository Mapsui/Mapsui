using Mapsui.Geometries;
using System;
using System.ComponentModel;
using System.Net;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
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
            return new Point(
                coordinateInPixels.X / PixelDensity,
                coordinateInPixels.Y / PixelDensity);
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

        public void NavigateToFullEnvelope(ScaleMethod scaleMethod)
        {
            _map?.NavigateTo(_map.Envelope, scaleMethod);
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

            widget.HandleWidgetTouched(screenPosition);
        }
    }
}
