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
            DefaultExtent = () => Envelope;
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
        /// Viewport holding informations about visible part of the map
        /// </summary>
        public Viewport Viewport { get; }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determin resolution</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            Viewport.Resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, Viewport.Width, Viewport.Height, scaleMethod);
            Viewport.Center = extent.Centroid;
            OnRefreshGraphics();
            RefreshData(true);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        public void NavigateTo(double resolution)
        {
            Viewport.Resolution = resolution;
            OnRefreshGraphics();
            RefreshData(true);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        public void NavigateTo(Point center)
        {
            Viewport.Center = center;
            OnRefreshGraphics();
            RefreshData(true);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        public void NavigateTo(double x, double y)
        {
            Viewport.Center = new Point(x, y);
            OnRefreshGraphics();
            RefreshData(true);
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        public void RotateTo(double rotation)
        {
            Viewport.Rotation = rotation;
            OnRefreshGraphics();
            RefreshData(true);
        }

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
                layer.RefreshData(Viewport.Extent, Viewport.Resolution, majorChange);
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

        // todo:
        // Evaluate if this works out. Perhaps we should pass a viewport and 
        // let users set the viewport. Adding the navigate methods to the Viewport
        // would make sense for that scenario.
        public Func<BoundingBox> DefaultExtent { get; set; }
    }
}