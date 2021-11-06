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
        object? Tag { get; set; }

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
        /// Gets the MRect of the entire layer. Can be null if there are no features in the layer.
        /// </summary>
        MRect? Extent { get; }

        /// <summary>
        /// Gets or sets rendering style of layer
        /// </summary>
        IStyle? Style { get; set; }

        /// <summary>
        /// Opacity of layer
        /// </summary>
        double Opacity { get; set; }

        /// <summary>
        /// Flag, if layer is busy
        /// </summary>
        bool Busy { get; set; }

        /// <summary>
        /// Get all features in a given MRect for a given resolution
        /// </summary>
        /// <param name="extent">Bounding box</param>
        /// <param name="resolution">Resolution of viewport</param>
        /// <returns></returns>
        IEnumerable<IFeature> GetFeatures(MRect extent, double resolution);

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
        /// If Discrete an implementation should always refresh it's data. If Continuous the
        /// implementation could ignore it. Example: During dragging a map a WMS layer would not want
        /// to fetch data, only on the drag end.
        /// <param name="fetchInfo">FetchInfo</param>
        void RefreshData(FetchInfo fetchInfo);

        /// <summary>
        /// To indicate the data withing the layer has changed. This triggers a DataChanged event.
        /// This is necessary for situations where the layer can not know about changes to it's data
        /// as in the case of editing of a geometry.
        /// </summary>
        void DataHasChanged();
    }
}