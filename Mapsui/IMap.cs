using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui
{
    public interface IMap
    {
        /// <summary>
        /// List of Widgets belonging to map
        /// </summary>
        List<IWidget> Widgets { get; }

        /// <summary>
        /// Projection type of Map. Normally in format like "EPSG:3857"
        /// </summary>
        string CRS { get; set; }

        /// <summary>
        /// Transformation to use for the different coordinate systems
        /// </summary>
        ITransformation Transformation { get; set; }

        /// <summary>
        /// A collection of layers. The first layer in the list is drawn first, the last one on top.
        /// </summary>
        LayerCollection Layers { get; }

        /// <summary>
        /// Map background color (defaults to transparent)
        ///  </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Gets the extents of the map based on the extents of all the layers in the layers collection
        /// </summary>
        /// <returns>Full map extents</returns>
        BoundingBox Envelope { get; }

        /// <summary>
        /// List of all native resolutions of this map
        /// </summary>
        IReadOnlyList<double> Resolutions { get; }

        Action<INavigator> Home { get; set; }

        /// <summary>
        /// Called whenever a property changed
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// DataChanged should be triggered by any data changes of any of the child layers
        /// </summary>
        event DataChangedEventHandler DataChanged;

        /// <summary>
        /// Abort fetching of all layers
        /// </summary>
        void AbortFetch();

        /// <summary>
        /// Clear cache of all layers
        /// </summary>
        void ClearCache();

        void RefreshData(BoundingBox extent, double resolution, bool majorChange);
    }
}
