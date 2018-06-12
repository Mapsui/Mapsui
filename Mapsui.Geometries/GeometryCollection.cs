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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     A GeometryCollection is a geometry that is a collection of 1 or more geometries.
    /// </summary>
    /// <remarks>
    ///     All the elements in a GeometryCollection must be in the same Spatial Reference. This is also the Spatial
    ///     Reference for the GeometryCollection.<br />
    ///     GeometryCollection places no other constraints on its elements. Subclasses of GeometryCollection may
    ///     restrict membership based on dimension and may also place other constraints on the degree of spatial overlap
    ///     between elements.
    /// </remarks>
    public class GeometryCollection : Geometry, IGeometryCollection, IEnumerable<Geometry>
    {
        /// <summary>
        ///     Initializes a new GeometryCollection
        /// </summary>
        public GeometryCollection()
        {
            Collection = new Collection<Geometry>();
        }

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="index">Geometry index</param>
        /// <returns>Geometry</returns>
        public Geometry this[int index] => Collection[index];

        /// <summary>
        ///     Gets or sets the GeometryCollection
        /// </summary>
        public IList<Geometry> Collection { get; set; }

        /// <summary>
        ///     Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<Geometry> GetEnumerator()
        {
            foreach (var g in Collection)
            {
                yield return g;
            }
        }

        /// <summary>
        ///     Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var g in Collection)
            {
                yield return g;
            }
        }

        /// <summary>
        ///     Gets the number of geometries in the collection.
        /// </summary>
        public virtual int NumGeometries => Collection.Count;

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="n">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public virtual Geometry Geometry(int n)
        {
            return Collection[n];
        }

        /// <summary>
        ///     Returns empty of all the geometries are empty or the collection is empty
        /// </summary>
        /// <returns>true of collection is empty</returns>
        public override bool IsEmpty()
        {
            if (Collection == null)
                return true;

            foreach (var geometry in Collection)
            {
                if (!geometry.IsEmpty())
                    return false;
            }

            return true;
        }

        [Obsolete("Use the BoundingBox field instead")]
        public new BoundingBox GetBoundingBox()
        {
            return BoundingBox;
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry, returned as a BoundingBox.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox BoundingBox
        {
            get
            {
                if (Collection.Count == 0)
                    return null;
                var b = this[0].BoundingBox;
                foreach (var geometry in Collection)
                {
                    b = b.Join(geometry.BoundingBox);
                }

                return b;
            }
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
        ///     Determines whether this GeometryCollection is spatially equal to the GeometryCollection 'g'
        /// </summary>
        /// <param name="g"></param>
        /// <returns>True if the GeometryCollections are equals</returns>
        public bool Equals(GeometryCollection g)
        {
            if (g == null)
                return false;
            if (g.Collection.Count != Collection.Count)
                return false;
            for (var i = 0; i < g.Collection.Count; i++)
            {
                if (!g.Collection[i].Equals(Collection[i]))
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
            var hash = 0;
            foreach (var geometry in Collection)
            {
                hash = hash ^ geometry.GetHashCode();
            }
            return hash;
        }

        public new GeometryCollection Clone()
        {
            var geometryCollection = new GeometryCollection();

            foreach (var geometry in this)
            {
                geometryCollection.Collection.Add(geometry.Clone());
            }

            return geometryCollection;
        }


        public override bool Contains(Point point)
        {
            foreach (var geometry in Collection)
            {
                if (geometry.Contains(point)) return true;
            }
            return false;
        }

        public override bool Equals(Geometry geom)
        {
            var geometryCollection = geom as GeometryCollection;
            if (geometryCollection == null) return false;
            return Equals(geometryCollection);
        }
    }
}