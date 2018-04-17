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

        public List<IWidget> Widgets { get; } = new List<IWidget>();

        public PanMode PanMode { get; set; } = PanMode.KeepCenterWithinExtents;

        public ZoomMode ZoomMode { get; set; } = ZoomMode.KeepWithinResolutions;

        /// <summary>
        /// Set this property in combination KeepCenterWithinExtents or KeepViewportWithinExtents.
        /// If PanLimits is not set Map.Extent will be used as restricted extent.
        /// </summary>
        public BoundingBox PanLimits { get; set; }

        /// <summary>
        /// Pair of the limits for the resolutions (smallest and biggest). The resolution is kept between these values.
        /// </summary>
        public MinMax ZoomLimits { get; set; }

        public string CRS { get; set; }

        /// <summary>
        /// The maps coordinate system
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

        public IList<ILayer> InfoLayers { get; } = new List<ILayer>();

        public IList<ILayer> HoverLayers { get; } = new List<ILayer>();

        public Viewport Viewport { get; }

        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            Viewport.Resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, Viewport.Width, Viewport.Height, scaleMethod);
            Viewport.Center = extent.GetCentroid();

            OnRefreshGraphics();
            ViewChanged(true);
        }

        public void NavigateTo(double resolution)
        {
            Viewport.Resolution = resolution;
            OnRefreshGraphics();
            ViewChanged(true);
        }

        public void NavigateTo(Point center)
        {
            Viewport.Center = center;
            OnRefreshGraphics();
            ViewChanged(true);
        }

        public void NavigateTo(double x, double y)
        {
            Viewport.Center = new Point(x, y);
            OnRefreshGraphics();
            ViewChanged(true);
        }

        public void RotateTo(double rotation)
        {
            Viewport.Rotation = rotation;
            OnRefreshGraphics();
            ViewChanged(true);
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

        public IReadOnlyList<double> Resolutions { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// DataChanged should be triggered by any data changes of any of the child layers
        /// </summary>
        public event DataChangedEventHandler DataChanged;
        public event EventHandler RefreshGraphics;
        public event EventHandler<MapInfoEventArgs> Info;
        public event EventHandler<MapInfoEventArgs> Hover;

        private void LayersLayerRemoved(ILayer layer)
        {
            layer.AbortFetch();

            layer.DataChanged -= LayerDataChanged;
            layer.PropertyChanged -= LayerPropertyChanged;

            InfoLayers.Remove(layer);

            Resolutions = DetermineResolutions(Layers);

            OnPropertyChanged(nameof(Layers));
        }

        public bool InvokeInfo(Point screenPosition, Point startScreenPosition, float scale, ISymbolCache symbolCache,
            Action<IWidget, Point> widgetCallback, int numTaps)
        {
            var allWidgets = Layers.Select(l => l.Attribution).Where(a => a != null).Concat(Widgets).ToList();
            
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

        private void LayersLayerAdded(ILayer layer)
        {
            layer.DataChanged += LayerDataChanged;
            layer.PropertyChanged += LayerPropertyChanged;

            layer.Transformation = Transformation;
            layer.CRS = CRS;
            Resolutions = DetermineResolutions(Layers);
            OnPropertyChanged(nameof(Layers));
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

        private void OnRefreshGraphics()
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

        public void AbortFetch()
        {
            foreach (var layer in _layers.ToList())
            {
                layer.AbortFetch();
            }
        }

        public void ViewChanged(bool majorChange)
        {
            foreach (var layer in _layers.ToList())
            {
                layer.ViewChanged(majorChange, Viewport.Extent, Viewport.Resolution);
            }
        }

        public void ClearCache()
        {
            foreach (var layer in _layers)
            {
                layer.ClearCache();
            }
        }
    }
}