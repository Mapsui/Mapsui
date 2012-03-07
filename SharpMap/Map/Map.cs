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
using System.Linq;
using SharpMap.Fetcher;
using SharpMap.Styles;
using SharpMap.Layers;
using SharpMap.Geometries;

namespace SharpMap
{
    /// <summary>
    /// Map class
    /// </summary>
    /// <example>
    /// Creating a new map instance, adding layers and rendering the map:
    /// <code lang="C#">
    /// SharpMap.Map myMap = new SharpMap.Map(picMap.Size);
    /// myMap.MinimumZoom = 100;
    /// myMap.BackgroundColor = Color.White;
    /// 
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    ///    string ConnStr = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=myGisDb;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.PostGIS(ConnStr, "myTable", "the_geom", 32632);
    /// myLayer.FillStyle = new SolidBrush(Color.FromArgb(240,240,240)); //Applies to polygon types only
    ///    myLayer.OutlineStyle = new Pen(Color.Blue, 1); //Applies to polygon and linetypes only
    /// //Setup linestyle (applies to line types only)
    ///    myLayer.Style.Line.Width = 2;
    ///    myLayer.Style.Line.Color = Color.Black;
    ///    myLayer.Style.Line.EndCap = System.Drawing.Drawing2D.LineCap.Round; //Round end
    ///    myLayer.Style.Line.StartCap = layRailroad.LineStyle.EndCap; //Round start
    ///    myLayer.Style.Line.DashPattern = new float[] { 4.0f, 2.0f }; //Dashed linestyle
    ///    myLayer.Style.EnableOutline = true;
    ///    myLayer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //Render smooth lines
    ///    myLayer.MaxVisible = 40000;
    /// 
    /// myMap.Layers.Add(myLayer);
    /// // [add more layers...]
    /// 
    /// myMap.Center = new SharpMap.Geometries.Point(725000, 6180000); //Set center of map
    ///    myMap.Zoom = 1200; //Set zoom level
    /// myMap.Size = new System.Drawing.Size(300,200); //Set output size
    /// 
    /// System.Drawing.Image imgMap = myMap.GetMap(); //Renders the map
    /// </code>
    /// </example>
    public class Map : IDisposable
    {
        private double minimumZoom;
        private double maximumZoom;
        private readonly LayerCollection layers = new LayerCollection();
        public event DataChangedEventHandler DataChanged;
        public event FeedbackEventHandler Feedback;

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
            if (layer is IAsyncDataFetcher)
            {
                var asyncLayer = layer as IAsyncDataFetcher;
                asyncLayer.AbortFetch();
                asyncLayer.DataChanged -= AsyncLayerDataChanged;
                //OnDataChanged(this, new DataChangedEventArgs(null, false, new TileInfo()));
            }
            layer.Feedback -= LayerFeedback;
        }

        void LayersLayerAdded(ILayer layer)
        {
            if (layer is IAsyncDataFetcher)
            {
                var asyncLayer = layer as IAsyncDataFetcher;
                asyncLayer.DataChanged += AsyncLayerDataChanged;
                //OnDataChanged(this, new DataChangedEventArgs(null, false, new TileInfo()));
            }
            layer.Feedback += LayerFeedback;
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
                if (layer is IAsyncDataFetcher)
                {
                    var tempLayer = layer as IAsyncDataFetcher;
                    tempLayer.AbortFetch();
                }
            }
        }

        public void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            foreach (var layer in layers.ToList())
            {
                if (layer is IAsyncDataFetcher)
                {
                    (layer as IAsyncDataFetcher).ViewChanged(changeEnd, extent, resolution);
                }
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

        #region Properties
        
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
        public BoundingBox GetExtents()
        {
            if (layers.Count == 0)
                return null; //it think we should allow Extent to be null
            BoundingBox bbox = null;
            foreach (ILayer t in layers)
            {
                bbox = bbox == null ? t.Envelope : bbox.Join(t.Envelope);
            }
            return bbox;
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

        /// <summary>
        /// Maximum zoom amount allowed
        /// </summary>
        public double MaximumZoom
        {
            get { return maximumZoom; }
            set {
                if (value <= 0)
                    throw (new Exception("Maximum zoom must larger than 0"));
                maximumZoom = value; 
            }
        }

        #endregion

        public void ClearCache()
        {
            foreach (var layer in layers)
            {
                //todo: create generic interface
                if (layer is IAsyncDataFetcher)
                {
                    (layer as IAsyncDataFetcher).ClearCache();
                }
            }
        }
    }
}
