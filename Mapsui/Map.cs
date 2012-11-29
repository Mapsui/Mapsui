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
        private readonly LayerCollection layers = new LayerCollection();
        public event DataChangedEventHandler DataChanged;
        public event FeedbackEventHandler Feedback;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new map
        /// </summary>
        public Map()
        {
            BackColor = Color.White;
            layers = new LayerCollection();
            layers.LayerAdded += LayersLayerAdded;
            layers.LayerRemoved += LayersLayerRemoved;
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
            if (e.PropertyName == "Envelope")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
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
            foreach (var layer in layers.ToList())
            {
                layer.AbortFetch();
            }
        }

        public void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            foreach (var layer in layers.ToList())
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
            layers.Clear();
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
            get { return layers; }
        }

        /// <summary>
        /// Map background color (defaults to transparent)
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// Gets the extents of the map based on the extents of all the layers in the layers collection
        /// </summary>
        /// <returns>Full map extents</returns>
        public BoundingBox Envelope
        {
            get
            {
                if (layers.Count == 0) return null;

                BoundingBox bbox = null;
                foreach (var layer in layers)
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
                return baseLayer.Schema.Resolutions.Select(r => r.UnitsPerPixel).ToList();
            }
        }

        public void ClearCache()
        {
            foreach (var layer in layers)
            {
                layer.ClearCache();
            }
        }
    }
}
