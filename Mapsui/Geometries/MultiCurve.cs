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
    /// A MultiCurve is a one-dimensional GeometryCollection whose elements are Curves
    /// </summary>
    public abstract class MultiCurve : GeometryCollection
    {
        /// <summary>
        ///  The inherent dimension of this Geometry object, which must be less than or equal to the coordinate dimension.
        /// </summary>
        public override int Dimension
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns true if this MultiCurve is closed (StartPoint=EndPoint for each curve in this MultiCurve)
        /// </summary>
        public abstract bool IsClosed { get; }

        /// <summary>
        /// The Length of this MultiCurve which is equal to the sum of the lengths of the element Curves.
        /// </summary>
        public abstract double Length { get; }

        /// <summary>
        /// Returns the number of geometries in the collection.
        /// </summary>
        public new abstract int NumGeometries { get; }

        /// <summary>
        /// Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="N">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public new abstract Geometry Geometry(int N);
    }
}