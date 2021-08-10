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

namespace Mapsui.Geometries
{
    /// <summary>
    ///     A MultiPoint is a 0 dimensional geometric collection. The elements of a MultiPoint are
    ///     restricted to Points. The points are not connected or ordered.
    /// </summary>
    public class MultiPoint : GeometryCollection
    {
        /// <summary>
        ///     Initializes a new MultiPoint collection
        /// </summary>
        public MultiPoint()
        {
            Points = new Collection<Point>();
        }

        /// <summary>
        ///     Initializes a new MultiPoint collection
        /// </summary>
        public MultiPoint(IEnumerable<double[]> points)
        {
            Points = new Collection<Point>();
            foreach (var point in points)
            {
                Points.Add(new Point(point[0], point[1]));
            }
        }

        /// <summary>
        ///     Gets the n'th point in the MultiPoint collection
        /// </summary>
        /// <param name="n">Index in collection</param>
        /// <returns>Point</returns>
        public new Point this[int n] => Points[n];

        /// <summary>
        ///     Gets or sets the MultiPoint collection
        /// </summary>
        public IList<Point> Points { get; set; }

        /// <summary>
        ///     Returns the number of geometries in the collection.
        /// </summary>
        public override int NumGeometries => Points.Count;

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="n">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public new Point Geometry(int n)
        {
            return Points[n];
        }

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return (Points != null) && (Points.Count == 0);
        }

        /// <summary>
        ///     Returns the shortest distance between any two points in the two geometries
        ///     as calculated in the spatial reference system of this Geometry.
        /// </summary>
        /// <param name="point">Geometry to calculate distance to</param>
        /// <returns>Shortest distance between any two points in the two geometries</returns>
        public override double Distance(Point point)
        {
            var minDistance = double.MaxValue;
            foreach (var geometry in Collection)
            {
                minDistance = Math.Min(minDistance, geometry.Distance(point));
            }
            return minDistance;
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox BoundingBox
        {
            get
            {
                if (Points == null || Points.Count == 0) return null;

                var bbox = new BoundingBox(Points[0], Points[0]);
                for (var i = 1; i < Points.Count; i++)
                {
                    bbox.Min.X = Points[i].X < bbox.Min.X ? Points[i].X : bbox.Min.X;
                    bbox.Min.Y = Points[i].Y < bbox.Min.Y ? Points[i].Y : bbox.Min.Y;
                    bbox.Max.X = Points[i].X > bbox.Max.X ? Points[i].X : bbox.Max.X;
                    bbox.Max.Y = Points[i].Y > bbox.Max.Y ? Points[i].Y : bbox.Max.Y;
                }
                return bbox;
            }
        }

        /// <summary>
        ///     Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new MultiPoint Clone()
        {
            var geoms = new MultiPoint();
            foreach (var point in Points)
            {
                geoms.Points.Add(point.Clone());
            }
            return geoms;
        }

        /// <summary>
        ///     Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Geometry> GetEnumerator()
        {
            foreach (var p in Points)
            {
                yield return p;
            }
        }
    }
}