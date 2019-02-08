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
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui
{
    /// <summary>
    /// Map class
    /// </summary>
    /// <remarks>
    /// Map holds all map related infos like transformation, layers, widgets and so on.
    /// </remarks>
    public class Map : INotifyPropertyChanged, IMap
    {
        private LayerCollection _layers = new LayerCollection();
        private Color _backColor = Color.White;
        private IViewportLimiter _limiter = new ViewportLimiter();

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map()
        {
            BackColor = Color.White;
            Layers = new LayerCollection();
        }

        /// <summary>
        /// To register if the initial Home call has been done.
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// When true the user can not pan (move) the map.
        /// </summary>
        public bool PanLock { get; set; }

        /// <summary>
        /// When true the user an not rotate the map
        /// </summary>
        public bool ZoomLock { get; set; }

        /// <summary>
        /// When true the user can not zoom into the map
        /// </summary>
        public bool RotationLock { get; set; }

        /// <summary>
        /// List of Widgets belonging to map
        /// </summary>
        public List<IWidget> Widgets { get; } = new List<IWidget>();

        /// <summary>
        /// Limit the extent to which the user can navigate
        /// </summary>
        public IViewportLimiter Limiter
        {
            get => _limiter;
            set
            {
                if (!_limiter.Equals(value))
                {
                    _limiter = value;
                    OnPropertyChanged(nameof(Limiter));
                }
            }
        }

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
            get => _layers;
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

        [Obsolete("Use ILayer.IsMapInfoLayer instead", true)]
        public IList<ILayer> InfoLayers { get; } = new List<ILayer>();

        [Obsolete("Use your own hover event and call MapControl.GetMapInfo", true)]
        public IList<ILayer> HoverLayers { get; } = new List<ILayer>();

        /// <summary>
        /// Map background color (defaults to transparent)
        ///  </summary>
        public Color BackColor
        {
            get => _backColor;
            set
            {
                if (_backColor == value) return;
                _backColor = value;
                OnPropertyChanged(nameof(BackColor));
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

#pragma warning disable 67
        [Obsolete("Use PropertyChanged instead", true)]
        public event EventHandler RefreshGraphics;
#pragma warning restore 67

        [Obsolete("Use MapControl.Info instead", true)]
#pragma warning disable 67
        public event EventHandler<MapInfoEventArgs> Info;
#pragma warning restore 67

        [Obsolete("Use your own hover event instead and call MapControl.GetMapInfo", true)]
#pragma warning disable 67
        public event EventHandler<MapInfoEventArgs> Hover;
#pragma warning restore 67
        
        /// <summary>
        /// Abort fetching of all layers
        /// </summary>
        public void AbortFetch()
        {
            foreach (var layer in _layers.ToList())
            {
                if (layer is IAsyncDataFetcher asyncLayer) asyncLayer.AbortFetch();
            }
        }

        /// <summary>
        /// Clear cache of all layers
        /// </summary>
        public void ClearCache()
        {
            foreach (var layer in _layers)
            {
                if (layer is IAsyncDataFetcher asyncLayer) asyncLayer.ClearCache();
            }
        }

        public void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            foreach (var layer in _layers.ToList())
            {
                layer.RefreshData(extent, resolution, majorChange);
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

        private void LayersLayerRemoved(ILayer layer)
        {
            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
            }

            layer.DataChanged -= LayerDataChanged;
            layer.PropertyChanged -= LayerPropertyChanged;

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

        public Action<INavigator> Home { get; set; } = n => n.NavigateToFullEnvelope();

        public IEnumerable<IWidget> GetWidgetsOfMapAndLayers()
        {
            return Widgets.Concat(Layers.Select(l => l.Attribution)).Where(w => w != null).ToList();
        }
    }
}