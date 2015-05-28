// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
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
using Mapsui.Utilities;

namespace Mapsui
{
    /// <summary>
    /// Map class
    /// </summary>
    public class Map : IDisposable, INotifyPropertyChanged
    {
        private LayerCollection _layers = new LayerCollection();
        private bool _lock;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map()
        {
            BackColor = Color.White;
            Layers = new LayerCollection();
            Viewport =  new Viewport { Center = { X = double.NaN, Y = double.NaN }, Resolution = double.NaN };
        }

        public bool Lock
        {
            get { return _lock; }
            set
            {
                if (_lock == value) return;
                _lock = value;
                OnPropertyChanged("Lock");
            }
        }

        public string CRS { get; set; }

        /// <summary>
        /// The maps coördinate system
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

        public Viewport Viewport { get; private set; }

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
            Viewport.Center.X = x;
            Viewport.Center.Y = y;
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
        public Color BackColor { get; set; } 

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

        public IList<double> Resolutions 
        {
            get 
            { 
                var baseLayer = Layers.FirstOrDefault(l => l.Enabled && l is ITileLayer) as ITileLayer;
                if (baseLayer == null || baseLayer.Schema == null) return new List<double>();
                return baseLayer.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();
            }
        }

        /// <summary>
        /// Disposes 
        /// the map object
        /// </summary>
        public void Dispose()
        {
            AbortFetch();
            _layers.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// DataChanged should be triggered by any data changes of any of the child layers
        /// </summary>
        public event DataChangedEventHandler DataChanged;
        public event EventHandler RefreshGraphics;

        void LayersLayerRemoved(ILayer layer)
        {
            layer.AbortFetch();
            layer.DataChanged -= LayerDataChanged;
            layer.PropertyChanged -= LayerPropertyChanged;
        }

        void LayersLayerAdded(ILayer layer)
        {
            layer.DataChanged += LayerDataChanged;
            layer.PropertyChanged += LayerPropertyChanged;
            layer.Transformation = Transformation;
            layer.CRS = CRS;
        }
        
        void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(sender, e.PropertyName);
        }

        protected virtual void OnRefreshGraphics()
        {
            var handler = RefreshGraphics;
            if (handler != null) RefreshGraphics(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(object sender, string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(sender, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, name);
        }

        private void LayerDataChanged(object sender, DataChangedEventArgs e)
        {
            OnDataChanged(sender, e);
        }
        
        private void OnDataChanged(object sender, DataChangedEventArgs e)
        {
            if (DataChanged != null)
            {
                DataChanged(sender, e);
            }
        }

        public void AbortFetch()
        {
            foreach (var layer in _layers.ToList())
            {
                layer.AbortFetch();
            }
        }

        public void ViewChanged(bool changeEnd)
        {
            foreach (var layer in _layers.ToList())
            {
                layer.ViewChanged(changeEnd, Viewport.Extent, Viewport.RenderResolution);
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
