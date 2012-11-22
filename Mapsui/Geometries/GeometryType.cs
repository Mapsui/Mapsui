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
    /// Enumeration of Simple Features Geometry types
    /// </summary>
    public enum GeometryType2
    {
        /// <summary>
        /// Geometry is the root class of the hierarchy. Geometry is an abstract (non-instantiable) class.
        /// </summary>
        Geometry = 0,
        /// <summary>
        /// A Point is a 0-dimensional geometry and represents a single location in coordinate space.
        /// </summary>
        Point = 1,
        /// <summary>
        /// A curve is a one-dimensional geometric object usually stored as a sequence of points,
        /// with the subtype of curve specifying the form of the interpolation between points.
        /// </summary>
        Curve = 2,
        /// <summary>
        /// A LineString is a curve with linear interpolation between points. Each consecutive
        /// pair of points defines a line segment.
        /// </summary>
        LineString = 3,
        /// <summary>
        /// A Surface is a two-dimensional geometric object.
        /// </summary>
        Surface = 4,
        /// <summary>
        /// A Polygon is a planar surface, defined by 1 exterior boundary and 0 or more interior
        /// boundaries. Each interior boundary defines a hole in the polygon.
        /// </summary>
        Polygon = 5,
        /// <summary>
        /// A GeometryCollection is a geometry that is a collection of 1 or more geometries.
        /// </summary>
        GeometryCollection = 6,
        /// <summary>
        /// A MultiPoint is a 0 dimensional geometric collection. The elements of a MultiPoint
        /// are restricted to Points. The points are not connected or ordered.
        /// </summary>
        MultiPoint = 7,
        /// <summary>
        /// A MultiCurve is a one-dimensional GeometryCollection whose elements are Curves.
        /// </summary>
        MultiCurve = 8,
        /// <summary>
        /// A MultiLineString is a MultiCurve whose elements are LineStrings.
        /// </summary>
        MultiLineString = 9,
        /// <summary>
        /// A MultiSurface is a two-dimensional geometric collection whose elements are
        /// surfaces. The interiors of any two surfaces in a MultiSurface may not intersect.
        /// The boundaries of any two elements in a MultiSurface may intersect at most at a
        /// finite number of points.
        /// </summary>
        MultiSurface = 10,
        /// <summary>
        /// A MultiPolygon is a MultiSurface whose elements are Polygons.
        /// </summary>
        MultiPolygon = 11,
    }
}