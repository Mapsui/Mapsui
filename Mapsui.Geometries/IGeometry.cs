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

using System;

namespace Mapsui.Geometries
{
    /// <summary>
    ///     Defines basic interface for a Geometry
    /// </summary>
    public interface IGeometry
    {
        /// <summary>
        ///     The minimum bounding box for this Geometry, returned as a <see cref="Geometry" />. The
        ///     polygon is defined by the corner points of the bounding box ((MINX, MINY), (MAXX, MINY), (MAXX,
        ///     MAXY), (MINX, MAXY), (MINX, MINY)).
        /// </summary>
        Geometry Envelope { get; }

        /// <summary>
        ///     The minimum <see cref="Geometries.BoundingBox" /> for this <see cref="Geometry" />.
        /// </summary>
        /// <returns><see cref="Geometries.BoundingBox" /> for this <see cref="Geometry" /></returns>
        BoundingBox BoundingBox { get; }

        [Obsolete("Use the BoundingBox field instead")]
        BoundingBox GetBoundingBox();

        /// <summary>
        ///     Exports this <see cref="Geometry" /> to a specific well-known text representation of <see cref="Geometry" />.
        /// </summary>
        string AsText();

        /// <summary>
        ///     Exports this <see cref="Geometry" /> to a specific well-known binary representation of <see cref="Geometry" />.
        /// </summary>
        byte[] AsBinary();

        /// <summary>
        ///     Returns a WellKnownText representation of the <see cref="Geometry" />
        /// </summary>
        /// <returns>Well-known text</returns>
        string ToString();

        /// <summary>
        ///     If true, then this <see cref="Geometry" /> represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this <see cref="Geometry" /> is the empty geometry</returns>
        bool IsEmpty();

        /// <summary>
        ///     This method must be overridden using 'public new [derived_data_type] Clone()'
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        Geometry Clone();

        /// <summary>
        ///     Returns 'true' if this <see cref="Geometry" /> is 'spatially equal' to another <see cref="Geometry" />
        /// </summary>
        bool Equals(Geometry geom);

        /// <summary>
        ///     Returns the shortest distance between any two points in the two geometries
        ///     as calculated in the spatial reference system of this <see cref="Geometry" />.
        /// </summary>
        /// <param name="point"><see cref="Point" /> to calculate distance to</param>
        /// <returns>Shortest distance between any two points in the two geometries</returns>
        double Distance(Point point);

        bool Contains(Point point);
    }
}