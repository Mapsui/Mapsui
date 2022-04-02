// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    /// <summary>
    /// Interface for data providers
    /// </summary>
    public interface IAsyncProvider<out T> : IProvider<T> where T : IFeature
    {
        IAsyncEnumerable<T> GetFeaturesAsync(FetchInfo fetchInfo);
    }
}