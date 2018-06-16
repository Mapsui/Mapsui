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
using System.Collections.ObjectModel;
using Mapsui.Geometries.Utilities;

namespace Mapsui.Geometries
{
    /// <summary>
    ///     A MultiLineString is a MultiCurve whose elements are LineStrings.
    /// </summary>
    public class MultiLineString : GeometryCollection
    {
        /// <summary>
        ///     Initializes an instance of a MultiLineString
        /// </summary>
        public MultiLineString()
        {
            LineStrings = new Collection<LineString>();
        }

        /// <summary>
        ///     Collection of <see cref="LineString">LineStrings</see> in the <see cref="MultiLineString" />
        /// </summary>
        public IList<LineString> LineStrings { get; set; }

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="index">Geometry index</param>
        /// <returns>Geometry at index</returns>
        public new LineString this[int index] => LineStrings[index];

        /// <summary>
        ///     Returns true if all LineStrings in this MultiLineString is closed (StartPoint=EndPoint for each LineString in this
        ///     MultiLineString)
        /// </summary>
        public bool IsClosed
        {
            get
            {
                foreach (var lineString in LineStrings)
                {
                    if (!lineString.IsClosed)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        ///     The length of this MultiLineString which is equal to the sum of the lengths of the element LineStrings.
        /// </summary>
        public double Length
        {
            get
            {
                double l = 0;
                foreach (var lineString in LineStrings)
                {
                    l += lineString.Length;
                }
                return l;
            }
        }

        /// <summary>
        ///     Returns the number of geometries in the collection.
        /// </summary>
        public override int NumGeometries => LineStrings.Count;

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            if ((LineStrings == null) || (LineStrings.Count == 0))
                return true;

            foreach (var lineString in LineStrings)
            {
                if (!lineString.IsEmpty())
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns the shortest distance between any two points in the two geometries
        ///     as calculated in the spatial reference system of this Geometry.
        /// </summary>
        /// <param name="point">Geometry to calculate distance to</param>
        /// <returns>Shortest distance between any two points in the two geometries</returns>
        public override double Distance(Point point)
        {
            // brute force approach!
            var minDist = double.MaxValue;
            foreach (var ls in LineStrings)
            {
                IList<Point> coord0 = ls.Vertices;
                for (var i = 0; i < coord0.Count - 1; i++)
                {
                    var dist = CGAlgorithms.DistancePointLine(point, coord0[i], coord0[i + 1]);
                    if (dist < minDist)
                        minDist = dist;
                }
            }
            return minDist;
        }

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="n">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public override Geometry Geometry(int n)
        {
            return LineStrings[n];
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox BoundingBox
        {
            get
            {
                if (LineStrings == null || LineStrings.Count == 0)
                    return null;
                var bbox = LineStrings[0].BoundingBox;
                for (var i = 1; i < LineStrings.Count; i++)
                {
                    bbox = bbox.Join(LineStrings[i].BoundingBox);
                }

                return bbox;
            }
        }

        /// <summary>
        ///     Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new MultiLineString Clone()
        {
            var geoms = new MultiLineString();
            foreach (var lineString in LineStrings)
            {
                geoms.LineStrings.Add(lineString.Clone());
            }
            return geoms;
        }

        /// <summary>
        ///     Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Geometry> GetEnumerator()
        {
            foreach (var l in LineStrings)
            {
                yield return l;
            }
        }
    }
}