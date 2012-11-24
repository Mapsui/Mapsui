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
using SharpMap.Utilities;

namespace SharpMap.Geometries
{
    /// <summary>
    /// A Polygon is a planar Surface, defined by 1 exterior boundary and 0 or more interior boundaries. Each
    /// interior boundary defines a hole in the Polygon.
    /// </summary>
    /// <remarks>
    /// Vertices of rings defining holes in polygons are in the opposite direction of the exterior ring.
    /// </remarks>
    public class Polygon : Surface
    {
        private LinearRing _ExteriorRing;
        private IList<LinearRing> _InteriorRings;

        /// <summary>
        /// Instatiates a polygon based on one extorier ring and a collection of interior rings.
        /// </summary>
        /// <param name="exteriorRing">Exterior ring</param>
        /// <param name="interiorRings">Interior rings</param>
        public Polygon(LinearRing exteriorRing, IList<LinearRing> interiorRings)
        {
            _ExteriorRing = exteriorRing;
            _InteriorRings = interiorRings;
        }

        /// <summary>
        /// Instatiates a polygon based on one extorier ring.
        /// </summary>
        /// <param name="exteriorRing">Exterior ring</param>
        public Polygon(LinearRing exteriorRing) : this(exteriorRing, new Collection<LinearRing>())
        {
        }

        /// <summary>
        /// Instatiates a polygon
        /// </summary>
        public Polygon() : this(new LinearRing(), new Collection<LinearRing>())
        {
        }

        /// <summary>
        /// Gets or sets the exterior ring of this Polygon
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        public LinearRing ExteriorRing
        {
            get { return _ExteriorRing; }
            set { _ExteriorRing = value; }
        }

        /// <summary>
        /// Gets or sets the interior rings of this Polygon
        /// </summary>
        public IList<LinearRing> InteriorRings
        {
            get { return _InteriorRings; }
            set { _InteriorRings = value; }
        }

        /// <summary>
        /// Returns the number of interior rings in this Polygon
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        /// <returns></returns>
        public int NumInteriorRing
        {
            get { return _InteriorRings.Count; }
        }

        /// <summary>
        /// The area of this Surface, as measured in the spatial reference system of this Surface.
        /// </summary>
        public override double Area
        {
            get
            {
                double area = 0.0;
                area += _ExteriorRing.Area;
                bool extIsClockwise = _ExteriorRing.IsCCW();
                for (int i = 0; i < _InteriorRings.Count; i++)
                    //opposite direction of exterior subtracts area
                    if (_InteriorRings[i].IsCCW() != extIsClockwise)
                        area -= _InteriorRings[i].Area;
                    else
                        area += _InteriorRings[i].Area;
                return area;
            }
        }

        /// <summary>
        /// The mathematical centroid for this Surface as a Point.
        /// The result is not guaranteed to be on this Surface.
        /// </summary>
        public override Point Centroid
        {
            get { return ExteriorRing.GetBoundingBox().GetCentroid(); }
        }

        /// <summary>
        /// A point guaranteed to be on this Surface.
        /// </summary>
        public override Point PointOnSurface
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the Nth interior ring for this Polygon as a LineString
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        /// <param name="N"></param>
        /// <returns></returns>
        public LinearRing InteriorRing(int N)
        {
            return _InteriorRings[N];
        }

        /// <summary>
        /// Transforms the polygon to image coordinates, based on the map
        /// </summary>
        /// <param name="map">Map to base coordinates on</param>
        /// <returns>Polygon in image coordinates</returns>
        public Point[] WorldToView(IView view)
        {

            int vertices = _ExteriorRing.Vertices.Count;
            for (int i = 0; i < _InteriorRings.Count; i++)
                vertices += _InteriorRings[i].Vertices.Count;

            Point[] v = new Point[vertices];
            for (int i = 0; i < _ExteriorRing.Vertices.Count; i++)
                v[i] = view.WorldToView(_ExteriorRing.Vertices[i]);
            int j = _ExteriorRing.Vertices.Count;
            for (int k = 0; k < _InteriorRings.Count; k++)
            {
                //The vertices of an interior polygon must be ordered counterclockwise to render as a hole
                //Uncomment the following lines if you are not sure of the orientation of your interior polygons
                //if (_InteriorRings[k].IsCCW())
                //{
                for (int i = 0; i < _InteriorRings[k].Vertices.Count; i++)
                    v[j + i] = view.WorldToView(_InteriorRings[k].Vertices[i]);
                //}
                //else
                //{
                //	for (int i = 1; i <= _InteriorRings[k].Vertices.Count; i++)
                //		v[j + i] = SharpMap.Utilities.Transform.WorldtoMap(_InteriorRings[k].Vertices[_InteriorRings[k].Vertices.Count - i - 1], map);
                //}
                j += _InteriorRings[k].Vertices.Count;
            }
            return v;
        }

        /// <summary>
        /// Returns the bounding box of the object
        /// </summary>
        /// <returns>bounding box</returns>
        public override BoundingBox GetBoundingBox()
        {
            if (_ExteriorRing == null || _ExteriorRing.Vertices.Count == 0) return null;
            BoundingBox bbox = new BoundingBox(_ExteriorRing.Vertices[0], _ExteriorRing.Vertices[0]);
            for (int i = 1; i < _ExteriorRing.Vertices.Count; i++)
            {
                bbox.Min.X = Math.Min(_ExteriorRing.Vertices[i].X, bbox.Min.X);
                bbox.Min.Y = Math.Min(_ExteriorRing.Vertices[i].Y, bbox.Min.Y);
                bbox.Max.X = Math.Max(_ExteriorRing.Vertices[i].X, bbox.Max.X);
                bbox.Max.Y = Math.Max(_ExteriorRing.Vertices[i].Y, bbox.Max.Y);
            }
            return bbox;
        }

        /// <summary>
        /// Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new Polygon Clone()
        {
            var p = new Polygon();
            p.ExteriorRing = _ExteriorRing.Clone();
            foreach (var t in _InteriorRings)
                p.InteriorRings.Add(t.Clone());
            return p;
        }

        #region "Inherited methods from abstract class Geometry"

        /// <summary>
        /// Determines if this Polygon and the specified Polygon object has the same values
        /// </summary>
        /// <param name="p">Polygon to compare with</param>
        /// <returns></returns>
        public bool Equals(Polygon p)
        {
            if (p == null)
                return false;
            if (!p.ExteriorRing.Equals(ExteriorRing))
                return false;
            if (p.InteriorRings.Count != InteriorRings.Count)
                return false;
            for (int i = 0; i < p.InteriorRings.Count; i++)
                if (!p.InteriorRings[i].Equals(InteriorRings[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="GetHashCode"/> is suitable for use 
        /// in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode"/>.</returns>
        public override int GetHashCode()
        {
            int hash = ExteriorRing.GetHashCode();
            
            foreach (var t in InteriorRings)
                hash = hash ^ t.GetHashCode();
            return hash;
        }

        /// <summary>
        /// If true, then this Geometry represents the empty point set, Ø, for the coordinate space. 
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return (ExteriorRing == null) || (ExteriorRing.Vertices.Count == 0);
        }

        /// <summary>
        /// Returns the closure of the combinatorial boundary of this Geometry. The
        /// combinatorial boundary is defined as described in section 3.12.3.2 of [1]. Because the result of this function
        /// is a closure, and hence topologically closed, the resulting boundary can be represented using
        /// representational geometry primitives
        /// </summary>
        public override Geometry Boundary()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the shortest distance between any two points in the two geometries
        /// as calculated in the spatial reference system of this Geometry.
        /// </summary>
        public override double Distance(Geometry geom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a geometry that represents the point set intersection of this Geometry
        /// with anotherGeometry.
        /// </summary>
        public override Geometry Intersection(Geometry geom)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}