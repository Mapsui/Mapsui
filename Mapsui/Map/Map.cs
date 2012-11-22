// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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
using SharpMap.Fetcher;
using SharpMap.Styles;
using SharpMap.Layers;
using SharpMap.Geometries;
using SharpMap.Projection;

namespace SharpMap
{
    /// <summary>
    /// Map class
    /// </summary>
    public class Map : IDisposable, INotifyPropertyChanged
    {
        private double minimumZoom;
        private double maximumZoom;
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
            maximumZoom = double.MaxValue;
            minimumZoom = 0;
            layers = new LayerCollection();
            layers.LayerAdded += LayersLayerAdded;
            layers.LayerRemoved += LayersLayerRemoved;
            Resolutions = new List<double>();
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

        /// <summary>
        /// Minimum zoom amount allowed
        /// </summary>
        public double MinimumZoom
        {
            get { return minimumZoom; }
            set {
                if (value < 0)
                    throw (new Exception("Minimum zoom must be 0 or more"));
                minimumZoom = value; 
            }
        }

        public IList<double> Resolutions { get; private set; } //todo: add way to assign resolutions based on a tile layer

        public void ClearCache()
        {
            foreach (var layer in layers)
            {
                layer.ClearCache();
            }
        }
    }
}
