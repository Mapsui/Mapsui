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

using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Layers
{
    /// <summary>
    /// Interface for map layers
    /// </summary>
    public interface ILayer : IAsyncDataFetcher, INotifyPropertyChanged 
    {
        int Id { get; }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this element
        /// </summary>
        object Tag { get; set; }

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
        string Name { get; set; }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        BoundingBox Envelope { get; }
        
        /// <summary>
        /// The spatial reference CRS. 
        /// This always needs to be equal to the CRS or the map. 
        /// It should eventually be removed altogether
        /// </summary>
        string CRS { get; set; }

        /// <summary>
        /// The coordinate transformation
        /// </summary>
        ITransformation Transformation { get; set; }

        IStyle Style { get; set; }

        /// <summary>
        /// Of all layers with Exclusive is true only one will be Enabled at a time.
        /// This can be used for radiobuttons.
        /// </summary>
        bool Exclusive { get; set; }

        double Opacity { get; set; }

        bool Busy { get; set; }

        IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution);

        /// <summary>
        /// Queries whether a layer supports projection to a certain CRS.
        /// </summary>
        /// <param name="crs">The crs to project to</param>
        /// <returns>True if is does, false if it does not, null if it is unknown</returns>
        bool? IsCrsSupported(string crs);

        Hyperlink Attribution { get; }

        IReadOnlyList<double> Resolutions { get; }
    }
}