// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using Mapsui.Geometries;
using System.Collections.Generic;

namespace Mapsui.Providers
{
    /// <summary>
    /// Interface for data providers
    /// </summary>
    public interface IProvider : IDisposable
    {
        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        string ConnectionId { get; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int SRID { get; set; }

        IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution);

        /// <summary>
        /// <see cref="Mapsui.Geometries.BoundingBox"/> of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        BoundingBox GetExtents();

        /// <summary>
        /// Opens the datasource
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the datasource
        /// </summary>
        void Close();
    }
}