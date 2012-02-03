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
using SharpMap.Geometries;
using SharpMap.Providers;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    /// <summary>
    /// Interface for map layers
    /// </summary>
    public interface ILayer
    {
        event FeedbackEventHandler Feedback;

        int Id { get; }
        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MinVisible { get; }

        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MaxVisible { get; }

        /// <summary>
        /// Specifies whether this layer should be rendered or not
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Name of layer
        /// </summary>
        string LayerName { get; set; }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        BoundingBox Envelope { get; }
        
        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int SRID { get; set; }

        IList<IStyle> Styles { get; set; }

        /// <summary>
        /// Of all layers with Exclusive is true only one will be Enabled at a time.
        /// This can be used for radiobuttons.
        /// </summary>
        bool Exclusive { get; set; }

        double Opacity { get; set; }

        IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution);
    }

    public delegate void FeedbackEventHandler(object sender, FeedbackEventArgs e);

    public class FeedbackEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
