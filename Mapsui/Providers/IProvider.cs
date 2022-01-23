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