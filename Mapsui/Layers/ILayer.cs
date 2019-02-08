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
    public interface ILayer : INotifyPropertyChanged 
    {
        /// <summary>
        /// Numerical Id of layer
        /// </summary>
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

        /// <summary>
        /// Gets or sets rendering style of layer
        /// </summary>
        IStyle Style { get; set; }

        /// <summary>
        /// Of all layers with Exclusive is true only one will be Enabled at a time.
        /// This can be used for radiobuttons.
        /// </summary>
        bool Exclusive { get; set; }

        /// <summary>
        /// Opacity of layer
        /// </summary>
        double Opacity { get; set; }

        /// <summary>
        /// Flag, if layer is busy
        /// </summary>
        bool Busy { get; set; }

        /// <summary>
        /// Get all features in a given BoundingBox for a given resolution
        /// </summary>
        /// <param name="extent">Bounding box</param>
        /// <param name="resolution">Resolution of viewport</param>
        /// <returns></returns>
        IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution);

        /// <summary>
        /// Queries whether a layer supports projection to a certain CRS.
        /// </summary>
        /// <param name="crs">The crs to project to</param>
        /// <returns>True if is does, false if it does not, null if it is unknown</returns>
        bool? IsCrsSupported(string crs);

        /// <summary>
        /// Attribution for layer
        /// </summary>
        Hyperlink Attribution { get; }

        /// <summary>
        /// List of native resolutions
        /// </summary>
        IReadOnlyList<double> Resolutions { get; }

        /// <summary>
        /// Indicates if the layer should be taken into account for the GetMapInfo request
        /// </summary>
        bool IsMapInfoLayer { get; set; }

        /// <summary>
        /// Event called when the data within the layer has changed allowing
        /// listeners to react to this.
        /// </summary>
        event DataChangedEventHandler DataChanged;

        /// <summary>
        /// Indicates that there has been a change in the view of the map
        /// </summary>
        /// <param name="extent">The new extent of the visible map</param>
        /// <param name="resolution">The new resolution of the visible map</param>
        /// <param name="majorChange">
        /// If true an implementation should always refresh it's data. If false (minorChange) the
        /// implementation could ignore it. Example: During dragging a map a WMS layer would not want
        /// to fetch data, only on the drag end.
        /// </param>
        void RefreshData(BoundingBox extent, double resolution, bool majorChange);

        /// <summary>
        /// To indicate the data withing the layer has changed. This triggers a DataChanged event.
        /// This is necessary for situations where the layer can not know about changes to it's data
        /// as in the case of editing of a geometry.
        /// </summary>
        void DataHasChanged();
    }
}