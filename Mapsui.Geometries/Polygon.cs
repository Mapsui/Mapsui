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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Geometries.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     A Polygon is a planar Surface, defined by 1 exterior boundary and 0 or more interior boundaries. Each
    ///     interior boundary defines a hole in the Polygon.
    /// </summary>
    /// <remarks>
    ///     Vertices of rings defining holes in polygons are in the opposite direction of the exterior ring.
    /// </remarks>
    public class Polygon : Geometry
    {
        /// <summary>
        ///     Instatiates a polygon based on one extorier ring and a collection of interior rings.
        /// </summary>
        /// <param name="exteriorRing">Exterior ring</param>
        /// <param name="interiorRings">Interior rings</param>
        public Polygon(LinearRing exteriorRing, IList<LinearRing> interiorRings)
        {
            ExteriorRing = exteriorRing;
            InteriorRings = interiorRings;
        }

        /// <summary>
        ///     Instatiates a polygon based on one extorier ring.
        /// </summary>
        /// <param name="exteriorRing">Exterior ring</param>
        public Polygon(LinearRing exteriorRing) : this(exteriorRing, new Collection<LinearRing>()) {}

        /// <summary>
        ///     Instatiates a polygon
        /// </summary>
        public Polygon() : this(new LinearRing(), new Collection<LinearRing>()) {}

        /// <summary>
        ///     Gets or sets the exterior ring of this Polygon
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        public LinearRing ExteriorRing { get; set; }

        /// <summary>
        ///     Gets or sets the interior rings of this Polygon
        /// </summary>
        public IList<LinearRing> InteriorRings { get; set; }

        /// <summary>
        ///     Returns the number of interior rings in this Polygon
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        /// <returns></returns>
        public int NumInteriorRing => InteriorRings.Count;

        /// <summary>
        ///     The area of this Surface, as measured in the spatial reference system of this Surface.
        /// </summary>
        public double Area
        {
            get
            {
                var area = 0.0;
                area += ExteriorRing.Area;
                var extIsClockwise = ExteriorRing.IsCCW();
                foreach (var linearRing in InteriorRings)
                {
                    if (linearRing.IsCCW() != extIsClockwise)
                        area -= linearRing.Area;
                    else
                        area += linearRing.Area;
                }
                return area;
            }
        }
        
        /// <summary>
        ///     Returns the Nth interior ring for this Polygon as a LineString
        /// </summary>
        /// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
        /// <param name="n"></param>
        /// <returns></returns>
        public LinearRing InteriorRing(int n)
        {
            return InteriorRings[n];
        }

        /// <summary>
        ///     Returns the bounding box of the object
        /// </summary>
        /// <returns>bounding box</returns>
        public override BoundingBox BoundingBox
        {
            get
            {
                if (ExteriorRing == null || ExteriorRing.Vertices.Count == 0) return null;

                var bbox = new BoundingBox(ExteriorRing.Vertices[0], ExteriorRing.Vertices[0]);
                for (var i = 1; i < ExteriorRing.Vertices.Count; i++)
                {
                    bbox.Min.X = Math.Min(ExteriorRing.Vertices[i].X, bbox.Min.X);
                    bbox.Min.Y = Math.Min(ExteriorRing.Vertices[i].Y, bbox.Min.Y);
                    bbox.Max.X = Math.Max(ExteriorRing.Vertices[i].X, bbox.Max.X);
                    bbox.Max.Y = Math.Max(ExteriorRing.Vertices[i].Y, bbox.Max.Y);
                }

                return bbox;
            }
        }

        /// <summary>
        ///     Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new Polygon Clone()
        {
            var polygon = new Polygon {ExteriorRing = ExteriorRing.Clone()};
            foreach (var interiorRing in InteriorRings)
            {
                polygon.InteriorRings.Add(interiorRing.Clone());
            }
            return polygon;
        }

        /// <summary>
        ///     Determines if this Polygon and the specified Polygon object has the same values
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
            for (var i = 0; i < p.InteriorRings.Count; i++)
            {
                if (!p.InteriorRings[i].Equals(InteriorRings[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="GetHashCode" /> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode" />.</returns>
        public override int GetHashCode()
        {
            var hash = ExteriorRing.GetHashCode();

            foreach (var ring in InteriorRings)
            {
                hash = hash ^ ring.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return (ExteriorRing == null) || (ExteriorRing.Vertices.Count == 0);
        }

        /// <summary>
        ///     Returns the shortest distance between any two points in the two geometries
        ///     as calculated in the spatial reference system of this Geometry.
        /// </summary>
        public override double Distance(Point point)
        {
            if (Contains(point)) return 0;

            return Algorithms.DistanceToLine(point, ExteriorRing.Vertices);
        }

        public override bool Contains(Point point)
        {
            return BoundingBox.Contains(point) && // First check bounds for performance
                Algorithms.PointInPolygon(ExteriorRing.Vertices, point);
        }
        
        public override bool Equals(Geometry geom)
        {
            var polygon = geom as Polygon;
            if (polygon == null) return false;
            return Equals(polygon);
        }

        public Polygon Rotate(double degrees, Point center)
        {
            var rotatedPolygon = Clone();
            rotatedPolygon.ExteriorRing = ExteriorRing.Rotate(degrees, center);
            for (var i = 0; i < InteriorRings.Count; i++)
            {
                rotatedPolygon.InteriorRings[i] = InteriorRings[i].Rotate(degrees, center);
            }

            return rotatedPolygon;
        }

        public Polygon Rotate(double degrees)
        {
            return Rotate(degrees, new Point(0, 0));
        }
    }
}