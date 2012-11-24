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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SharpMap.Geometries
{
    /// <summary>
    /// A MultiPolygon is a MultiSurface whose elements are Polygons.
    /// </summary>
    public class MultiPolygon : MultiSurface
    {
        private IList<Polygon> _Polygons;

        /// <summary>
        /// Instantiates a MultiPolygon
        /// </summary>
        public MultiPolygon()
        {
            _Polygons = new Collection<Polygon>();
        }

        /// <summary>
        /// Collection of polygons in the multipolygon
        /// </summary>
        public IList<Polygon> Polygons
        {
            get { return _Polygons; }
            set { _Polygons = value; }
        }

        /// <summary>
        /// Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="index">Geometry index</param>
        /// <returns>Geometry at index</returns>
        public new Polygon this[int index]
        {
            get { return _Polygons[index]; }
        }

        /// <summary>
        /// Returns summed area of the Polygons in the MultiPolygon collection
        /// </summary>
        public override double Area
        {
            get
            {
                double result = 0;
                for (int i = 0; i < _Polygons.Count; i++)
                    result += _Polygons[i].Area;
                return result;
            }
        }

        /// <summary>
        /// The mathematical centroid for the surfaces as a Point.
        /// The result is not guaranteed to be on any of the surfaces.
        /// </summary>
        public override Point Centroid
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// A point guaranteed to be on this Surface.
        /// </summary>
        public override Point PointOnSurface
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the number of geometries in the collection.
        /// </summary>
        public override int NumGeometries
        {
            get { return _Polygons.Count; }
        }

        /// <summary>
        /// If true, then this Geometry represents the empty point set, Ø, for the coordinate space. 
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            if (_Polygons == null || _Polygons.Count == 0)
                return true;
            for (int i = 0; i < _Polygons.Count; i++)
                if (!_Polygons[i].IsEmpty())
                    return false;
            return true;
        }

        /// <summary>
        /// Returns the closure of the combinatorial boundary of this Geometry. The
        /// combinatorial boundary is defined as described in section 3.12.3.2 of [1]. Because the result of this function
        /// is a closure, and hence topologically closed, the resulting boundary can be represented using
        /// representational geometry primitives
        /// </summary>
        /// <returns>Closure of the combinatorial boundary of this Geometry</returns>
        public override Geometry Boundary()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the shortest distance between any two points in the two geometries
        /// as calculated in the spatial reference system of this Geometry.
        /// </summary>
        /// <param name="geom">Geometry to calculate distance to</param>
        /// <returns>Shortest distance between any two points in the two geometries</returns>
        public override double Distance(Geometry geom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a geometry that represents the point set intersection of this Geometry
        /// with anotherGeometry.
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <returns>Returns a geometry that represents the point set intersection of this Geometry with anotherGeometry.</returns>
        public override Geometry Intersection(Geometry geom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="N">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public override Geometry Geometry(int N)
        {
            return _Polygons[N];
        }

        /// <summary>
        /// Returns the bounding box of the object
        /// </summary>
        /// <returns>bounding box</returns>
        public override BoundingBox GetBoundingBox()
        {
            if (_Polygons == null || _Polygons.Count == 0)
                return null;
            BoundingBox bbox = Polygons[0].GetBoundingBox();
            for (int i = 1; i < Polygons.Count; i++)
                bbox = bbox.Join(Polygons[i].GetBoundingBox());
            return bbox;
        }

        /// <summary>
        /// Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new MultiPolygon Clone()
        {
            MultiPolygon geoms = new MultiPolygon();
            for (int i = 0; i < _Polygons.Count; i++)
                geoms.Polygons.Add(_Polygons[i].Clone());
            return geoms;
        }

        /// <summary>
        /// Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Geometry> GetEnumerator()
        {
            foreach (Polygon p in _Polygons)
                yield return p;
        }
    }
}