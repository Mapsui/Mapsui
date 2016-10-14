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
using System.Linq;

namespace Mapsui.Geometries
{
    /// <summary>
    /// A LineString is a Curve with linear interpolation between points. Each consecutive pair of points defines a
    /// line segment.
    /// </summary>
    public class LineString : Curve
    {
        private List<Point> _vertices;

        /// <summary>
        /// Initializes an instance of a LineString from a set of vertices
        /// </summary>
        /// <param name="vertices"></param>
        public LineString(IEnumerable<Point> vertices)
        {
            _vertices = vertices.ToList();
        }

        /// <summary>
        /// Initializes an instance of a LineString
        /// </summary>
        public LineString() : this(new List<Point>())
        {
        }

        /// <summary>
        /// Initializes an instance of a LineString
        /// </summary>
        /// <param name="points"></param>
        public LineString(IEnumerable<double[]> points)
        {
            var vertices = new Collection<Point>();
            foreach (var point in points) vertices.Add(new Point(point));
            _vertices = vertices.ToList();
        }

        /// <summary>
        /// Gets or sets the collection of vertices in this Geometry
        /// </summary>
        public List<Point> Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        /// <summary>
        /// Returns the vertice where this Geometry begins
        /// </summary>
        /// <returns>First vertice in LineString</returns>
        public override Point StartPoint
        {
            get
            {
                if (_vertices.Count == 0)
                    throw new Exception("No startpoint found: LineString has no vertices.");
                return _vertices[0];
            }
        }

        /// <summary>
        /// Gets the vertice where this Geometry ends
        /// </summary>
        /// <returns>Last vertice in LineString</returns>
        public override Point EndPoint
        {
            get
            {
                if (_vertices.Count == 0)
                    throw new Exception("No endpoint found: LineString has no vertices.");
                return _vertices[_vertices.Count - 1];
            }
        }

        /// <summary>
        /// Returns true if this LineString is closed and simple
        /// </summary>
        public override bool IsRing => (IsClosed && IsSimple());

        /// <summary>
        /// The length of this LineString, as measured in the spatial reference system of this LineString.
        /// </summary>
        public override double Length
        {
            get
            {
                if (Vertices.Count < 2)
                    return 0;
                double sum = 0;
                for (int i = 1; i < Vertices.Count; i++)
                    sum += Vertices[i].Distance(Vertices[i - 1]);
                return sum;
            }
        }


        /// <summary>
        /// The number of points in this LineString.
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        public int NumPoints => _vertices.Count;

        /// <summary>
        /// Returns the specified point N in this Linestring.
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        /// <param name="n"></param>
        /// <returns></returns>
        public Point Point(int n)
        {
            return _vertices[n];
        }


        /// <summary>
        /// The position of a point on the line, parameterised by length.
        /// </summary>
        /// <param name="t">Distance down the line</param>
        /// <returns>Point at line at distance t from StartPoint</returns>
        public override Point Value(double t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns>BoundingBox for this geometry</returns>
        public override BoundingBox GetBoundingBox()
        {
            if (Vertices == null || Vertices.Count == 0)
                return null;
            var bbox = new BoundingBox(Vertices[0], Vertices[0]);
            for (int i = 1; i < Vertices.Count; i++)
            {
                bbox.Min.X = Vertices[i].X < bbox.Min.X ? Vertices[i].X : bbox.Min.X;
                bbox.Min.Y = Vertices[i].Y < bbox.Min.Y ? Vertices[i].Y : bbox.Min.Y;
                bbox.Max.X = Vertices[i].X > bbox.Max.X ? Vertices[i].X : bbox.Max.X;
                bbox.Max.Y = Vertices[i].Y > bbox.Max.Y ? Vertices[i].Y : bbox.Max.Y;
            }
            return bbox;
        }

        /// <summary>
        /// Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new LineString Clone()
        {
            var l = new LineString();
            foreach (Point vertex in _vertices)
                l.Vertices.Add(vertex.Clone());
            return l;
        }


        /// <summary>
        /// Checks whether this instance is spatially equal to the LineString 'l'
        /// </summary>
        /// <param name="lineString">LineString to compare to</param>
        /// <returns>true of the objects are spatially equal</returns>
        public bool Equals(LineString lineString)
        {
            if (lineString?.Vertices.Count != Vertices.Count)
                return false;
            for (int i = 0; i < lineString.Vertices.Count; i++)
                if (!lineString.Vertices[i].Equals(Vertices[i]))
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
            return Vertices.Aggregate(0, (current, t) => current ^ t.GetHashCode());
        }

        /// <summary>
        /// If true, then this Geometry represents the empty point set, Ø, for the coordinate space. 
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return _vertices == null || _vertices.Count == 0;
        }

        /// <summary>
        ///  Returns 'true' if this Geometry has no anomalous geometric points, such as self
        /// intersection or self tangency. The description of each instantiable geometric class will include the specific
        /// conditions that cause an instance of that class to be classified as not simple.
        /// </summary>
        /// <returns>true if the geometry is simple</returns>
        public bool IsSimple()
        {
            var verts = new Collection<Point>();

            foreach (Point vertex in _vertices)
                if (0 != verts.IndexOf(vertex))
                    verts.Add(vertex);

            return (verts.Count == _vertices.Count - (IsClosed ? 1 : 0));
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
    }
}