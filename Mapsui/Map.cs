// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;

namespace Mapsui
{
    /// <summary>
    /// Map class
    /// </summary>
    /// <remarks>
    /// Map holds all map related infos like transformation, layers, widgets and so on.
    /// </remarks>
    public class Map : INotifyPropertyChanged
    {
        private LayerCollection _layers = new LayerCollection();
        private Color _backColor = Color.White;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map()
        {
            BackColor = Color.White;
            Layers = new LayerCollection();
            Viewport = new Viewport { Center = { X = double.NaN, Y = double.NaN }, Resolution = double.NaN };
        }

        /// <summary>
        /// List of Widgets belonging to map
        /// </summary>
        public List<IWidget> Widgets { get; } = new List<IWidget>();

        /// <summary>
        /// Pan mode to use, when map is paned
        /// </summary>
        public PanMode PanMode { get; set; } = PanMode.KeepCenterWithinExtents;

        /// <summary>
        /// Zoom mode to use, when map is zoomed
        /// </summary>
        public ZoomMode ZoomMode { get; set; } = ZoomMode.KeepWithinResolutions;

        /// <summary>
        /// Set this property in combination KeepCenterWithinExtents or KeepViewportWithinExtents.
        /// If PanLimits is not set, Map.Extent will be used as restricted extent.
        /// </summary>
        public BoundingBox PanLimits { get; set; }

        /// <summary>
        /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
        /// to anything else than None, resolution is kept between these values.
        /// </summary>
        public MinMax ZoomLimits { get; set; }

        /// <summary>
        /// Projection type of Map. Normally in format like "EPSG:3857"
        /// </summary>
        public string CRS { get; set; }

        /// <summary>
        /// Transformation to use for the different coordinate systems
        /// </summary>
        public ITransformation Transformation { get; set; }

        /// <summary>
        /// A collection of layers. The first layer in the list is drawn first, the last one on top.
        /// </summary>
        public LayerCollection Layers
        {
            get { return _layers; }
            private set
            {
                var tempLayers = _layers;
                if (tempLayers != null)
                {
                    _layers.LayerAdded -= LayersLayerAdded;
                    _layers.LayerRemoved -= LayersLayerRemoved;
                }
                _layers = value;
                _layers.LayerAdded += LayersLayerAdded;
                _layers.LayerRemoved += LayersLayerRemoved;
            }
        }

        /// <summary>
        /// List of layers, that are respected when creating the info event
        /// </summary>
        public IList<ILayer> InfoLayers { get; } = new List<ILayer>();

        /// <summary>
        /// List of layers, that are respected when creating the hover event
        /// </summary>
        public IList<ILayer> HoverLayers { get; } = new List<ILayer>();

        /// <summary>
        /// Viewport holding informations about visible part of the map
        /// </summary>
        public Viewport Viewport { get; }

        [Obsolete("Use Viewport.NavigateTo()")]
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            Viewport.NavigateTo(extent, scaleMethod);
        }

        [Obsolete("Use Viewport.NavigateTo()")]
        public void NavigateTo(double resolution)
        {
            Viewport.NavigateTo(resolution);
        }

        [Obsolete("Use Viewport.NavigateTo()")]
        public void NavigateTo(Point center)
        {
            Viewport.NavigateTo(center);
        }

        [Obsolete("Use Viewport.NavigateTo()")]
        public void NavigateTo(double x, double y)
        {
            Viewport.NavigateTo(x, y);
        }

        [Obsolete("Use Viewport.NavigateTo()")]
        public void RotateTo(double rotation)
        {
            Viewport.RotateTo(rotation);
        }

        /// <summary>
        /// Map background color (defaults to transparent)
        ///  </summary>
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor == value)
                    return;
                _backColor = value;
                OnRefreshGraphics();
            }
        }

        /// <summary>
        /// Gets the extents of the map based on the extents of all the layers in the layers collection
        /// </summary>
        /// <returns>Full map extents</returns>
        public BoundingBox Envelope
        {
            get
            {
                if (_layers.Count == 0) return null;

                BoundingBox bbox = null;
                foreach (var layer in _layers)
                {
                    bbox = bbox == null ? layer.Envelope : bbox.Join(layer.Envelope);
                }
                return bbox;
            }
        }

        /// <summary>
        /// List of all native resolutions of this map
        /// </summary>
        public IReadOnlyList<double> Resolutions { get; private set; }

        /// <summary>
        /// Called whenever a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// DataChanged should be triggered by any data changes of any of the child layers
        /// </summary>
        public event DataChangedEventHandler DataChanged;

        /// <summary>
        /// Called whenever visible map needs an update
        /// </summary>
        public event EventHandler RefreshGraphics;

        /// <summary>
        ///  Called whenever a feature in one of the layers in InfoLayers is hitten by a click 
        /// </summary>
        public event EventHandler<MapInfoEventArgs> Info;

        /// <summary>
        /// Called whenever mouse is over a feature in one of the layers in HoverLayers
        /// </summary>
        public event EventHandler<MapInfoEventArgs> Hover;

        /// <summary>
        /// Check, if a widget or feature at a given screen position is clicked/tapped
        /// </summary>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="startScreenPosition">Screen position of Viewport/MapControl</param>
        /// <param name="scale">Scale of scrren. Normally is 1, but could be greater.</param>
        /// <param name="symbolCache">Cache for symbols to determin size</param>
        /// <param name="widgetCallback">Callback, which is called when Widget is hiten</param>
        /// <param name="numTaps">Number of clickes/taps</param>
        /// <returns>True, if something done </returns>
        public bool InvokeInfo(Point screenPosition, Point startScreenPosition, float scale, ISymbolCache symbolCache,
            Action<IWidget, Point> widgetCallback, int numTaps)
        {
            var layerWidgets = Layers.Select(l => l.Attribution).Where(a => a != null);
            var allWidgets = layerWidgets.Concat(Widgets).ToList(); // Concat layer widgets and map widgets.
            
            // First check if a Widget is clicked. In the current design they are always on top of the map.
            var widget = WidgetTouch.GetWidget(screenPosition, startScreenPosition, scale, allWidgets);
            if (widget != null)
            {
                // TODO how should widgetCallback have a handled type thing?
                // Widgets should be iterated through rather than getting a single widget, 
                // based on Z index and then called until handled = true; Ordered By highest Z

                widgetCallback(widget, new Point(screenPosition.X / scale, screenPosition.Y / scale));
                return true; 
            }

            if (Info == null) return false;
            var mapInfo = InfoHelper.GetMapInfo(Viewport, screenPosition, scale, InfoLayers, symbolCache);

            if (mapInfo != null)
            {
                // TODO Info items should be iterated through rather than getting a single item, 
                // based on Z index and then called until handled = true; Ordered By highest Z
                var mapInfoEventArgs = new MapInfoEventArgs
                {
                    MapInfo = mapInfo,
                    NumTaps = numTaps,
                    Handled = false
                };
                Info?.Invoke(this, mapInfoEventArgs);
                return mapInfoEventArgs.Handled;
            }

            return false;
        }

        private MapInfoEventArgs _previousHoverEventArgs;

        /// <summary>
        ///  Check, if mouse is hovered over a feature at a given screen position
        /// </summary>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="scale">Scale of scrren. Normally is 1, but could be greater.</param>
        /// <param name="symbolCache">Cache for symbols to determin size</param>
        public void InvokeHover(Point screenPosition, float scale, ISymbolCache symbolCache)
        {
            if (Hover == null) return;
            if (HoverLayers.Count == 0) return;
            var mapInfo = InfoHelper.GetMapInfo(Viewport, screenPosition, scale, HoverLayers, symbolCache);

            if (mapInfo?.Feature != _previousHoverEventArgs?.MapInfo.Feature) // only notify when the feature changes
            {
                var mapInfoEventArgs = new MapInfoEventArgs
                {
                    MapInfo = mapInfo,
                    NumTaps = 0,
                    Handled = false
                };
                
                _previousHoverEventArgs = mapInfoEventArgs;
                Hover?.Invoke(this, mapInfoEventArgs);
            }
        }

        /// <summary>
        /// Abort fetching of all layers
        /// </summary>
        public void AbortFetch()
        {
            foreach (var layer in _layers.ToList())
            {
                layer.AbortFetch();
            }
        }

        /// <summary>
        /// Clear cache of all layers
        /// </summary>
        public void ClearCache()
        {
            foreach (var layer in _layers)
            {
                layer.ClearCache();
            }
        }

        public void RefreshData(bool majorChange)
        {
            foreach (var layer in _layers.ToList())
            {
                layer.ViewChanged(majorChange, Viewport.Extent, Viewport.Resolution);
            }
        }

        private void LayersLayerAdded(ILayer layer)
        {
            layer.DataChanged += LayerDataChanged;
            layer.PropertyChanged += LayerPropertyChanged;

            layer.Transformation = Transformation;
            layer.CRS = CRS;
            Resolutions = DetermineResolutions(Layers);
            OnPropertyChanged(nameof(Layers));

            OnRefreshGraphics();
        }

        private void LayersLayerRemoved(ILayer layer)
        {
            layer.AbortFetch();

            layer.DataChanged -= LayerDataChanged;
            layer.PropertyChanged -= LayerPropertyChanged;

            InfoLayers.Remove(layer);

            Resolutions = DetermineResolutions(Layers);

            OnPropertyChanged(nameof(Layers));

            OnRefreshGraphics();
        }

        private static IReadOnlyList<double> DetermineResolutions(IEnumerable<ILayer> layers)
        {
            var items = new Dictionary<double, double>();
            const float normalizedDistanceThreshold = 0.75f;
            foreach (var layer in layers)
            {
                if (!layer.Enabled || layer.Resolutions == null) continue;

                foreach (var resolution in layer.Resolutions)
                {
                    // About normalization:
                    // Resolutions don't have equal distances because they 
                    // are multiplied by two at every step. Distances on the 
                    // lower zoom levels have very different meaning than on the
                    // higher zoom levels. So we work with a normalized resolution
                    // to determine if another resolution adds value. If a resolution
                    // is a factor of 2 of another resolution. The normalized distance
                    // is one.
                    var normalized = Math.Log(resolution, 2);
                    if (items.Count == 0)
                    {
                        items[normalized] = resolution;
                    }
                    else
                    {
                        var normalizedDistance = items.Keys.Min(k => Math.Abs(k - normalized));
                        if (normalizedDistance > normalizedDistanceThreshold) items[normalized] = resolution;
                    }
                }
            }

            return items.Select(i => i.Value).OrderByDescending(i => i).ToList();
        }

        private void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(sender, e.PropertyName);
        }

        internal void OnRefreshGraphics()
        {
            RefreshGraphics?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, name);
        }

        private void LayerDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(sender, e);
        }

        private void OnDataChanged(object sender, DataChangedEventArgs e)
        {
            DataChanged?.Invoke(sender, e);
        }
    }
}