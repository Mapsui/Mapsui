// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

namespace SharpMap.Geometries
{
    /// <summary>
    /// A MultiSurface is a two-dimensional geometric collection whose elements are surfaces. The interiors of any
    /// two surfaces in a MultiSurface may not intersect. The boundaries of any two elements in a MultiSurface may
    /// intersect at most at a finite number of points.
    /// </summary>
    /// <remarks>
    /// MultiSurface is a non-instantiable class in this specification, it defines a set of methods for its subclasses and
    /// is included for reasons of extensibility. The instantiable subclass of MultiSurface is MultiPolygon,
    /// corresponding to a collection of Polygons.
    /// </remarks>
    public abstract class MultiSurface : GeometryCollection
    {
        /// <summary>
        /// The area of this Surface, as measured in the spatial reference system of this Surface.
        /// </summary>
        public abstract double Area { get; }

        /// <summary>
        /// The mathematical centroid for this Surface as a Point.
        /// The result is not guaranteed to be on this Surface.
        /// </summary>
        public abstract Point Centroid { get; }

        /// <summary>
        /// A point guaranteed to be on this Surface.
        /// </summary>
        public abstract Point PointOnSurface { get; }

        /// <summary>
        ///  The inherent dimension of this Geometry object, which must be less than or equal to the coordinate dimension.
        /// </summary>
        public override int Dimension
        {
            get { return 2; }
        }
    }
}