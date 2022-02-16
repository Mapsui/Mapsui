// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    /// <summary>
    /// Interface for data providers
    /// </summary>
    public interface IProvider<out T> where T : IFeature
    {
        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        string? CRS { get; set; }

        IEnumerable<T> GetFeatures(FetchInfo fetchInfo);

        /// <summary>
        /// <see cref="Mapsui.Geometries.BoundingBox"/> of data set
        /// </summary>
        /// <returns>BoundingBox</returns>
        MRect? GetExtent();
    }
}