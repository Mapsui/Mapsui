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
using Mapsui.Styles;
using Mapsui.Layers;
using Mapsui.Geometries;
using Mapsui.Projection;

namespace Mapsui
{
    /// <summary>
    /// Map class
    /// </summary>
    public class Map : IDisposable, INotifyPropertyChanged
    {
        private LayerCollection _layers = new LayerCollection();
        public event DataChangedEventHandler DataChanged;
        public event FeedbackEventHandler Feedback;
        public event PropertyChangedEventHandler PropertyChanged;
        private NotifyingViewport _viewport;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map()
        {
            BackColor = Color.White;
            Layers = new LayerCollection();
            Viewport =  new NotifyingViewport { Center = { X = double.NaN, Y = double.NaN }, Resolution = double.NaN };
        }

        void LayersLayerRemoved(ILayer layer)
        {
            layer.AbortFetch();
            layer.DataChanged -= AsyncLayerDataChanged;
            layer.Feedback -= LayerFeedback;
            layer.PropertyChanged -= LayerPropertyChanged;
        }

        void LayersLayerAdded(ILayer layer)
        {
            layer.DataChanged += AsyncLayerDataChanged;
            layer.Feedback += LayerFeedback;
            layer.PropertyChanged += LayerPropertyChanged;
            layer.Transformation = Transformation;
        }

        void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(sender, e.PropertyName);
        }

        private void OnPropertyChanged(object sender, string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, name);
        }

        private void LayerFeedback(object sender, FeedbackEventArgs e)
        {
            OnFeedback(sender, e);
        }

        private void OnFeedback(object sender, FeedbackEventArgs e)
        {
            if (Feedback != null)
            {
                Feedback(sender, e);
            }
        }

        private void AsyncLayerDataChanged(object sender, DataChangedEventArgs e)
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

        public void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            foreach (var layer in _layers.ToList())
            {
                layer.ViewChanged(changeEnd, extent, resolution);
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
            set
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

        public NotifyingViewport Viewport
        {
            set
            {
                var tempViewport = _viewport;
                if (tempViewport != null)
                {
                    _viewport.PropertyChanged -= ViewportOnPropertyChanged;
                }
                _viewport = value;
                _viewport.PropertyChanged += ViewportOnPropertyChanged;
            }
            get { return _viewport; }
        }

        private void ViewportOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(sender, e.PropertyName);
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
                if (baseLayer == null) return new List<double>();
                return baseLayer.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();
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
