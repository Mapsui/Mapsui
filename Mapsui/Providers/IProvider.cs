// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Providers;

/// <summary>
/// Interface for data providers
/// </summary>
public interface IProvider
{
    /// <summary>
    /// The spatial reference ID (CRS)
    /// </summary>
    string? CRS { get; set; }

    /// <summary>
    /// <see cref="MRect"/> of data set
    /// </summary>
    /// <returns>BoundingBox</returns>
    MRect? GetExtent();

    Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo);
}
