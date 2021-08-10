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

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Mapsui.Geometries.WellKnownBinary
{
    /// <summary>
    ///     Converts Well-known Binary representations to a <see cref="Mapsui.Geometries.Geometry" /> instance.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The Well-known Binary Representation for <see cref="Mapsui.Geometries.Geometry" /> (WKBGeometry) provides a
    ///         portable
    ///         representation of a <see cref="Mapsui.Geometries.Geometry" /> value as a contiguous stream of bytes. It permits
    ///         <see cref="Mapsui.Geometries.Geometry" />
    ///         values to be exchanged between an ODBC client and an SQL database in binary form.
    ///     </para>
    ///     <para>
    ///         The Well-known Binary Representation for <see cref="Mapsui.Geometries.Geometry" /> is obtained by serializing a
    ///         <see cref="Mapsui.Geometries.Geometry" />
    ///         instance as a sequence of numeric types drawn from the set {Unsigned Integer, Double} and
    ///         then serializing each numeric type as a sequence of bytes using one of two well defined,
    ///         standard, binary representations for numeric types (NDR, XDR). The specific binary encoding
    ///         (NDR or XDR) used for a geometry byte stream is described by a one byte tag that precedes
    ///         the serialized bytes. The only difference between the two encodings of geometry is one of
    ///         byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.
    ///     </para>
    /// </remarks>
    public static class GeometryFromWKB
    {
        /// <summary>
        ///     Creates a <see cref="Mapsui.Geometries.Geometry" /> from the supplied byte[] containing the Well-known Binary
        ///     representation.
        /// </summary>
        /// <param name="bytes">byte[] containing the Well-known Binary representation.</param>
        /// <returns>A <see cref="Mapsui.Geometries.Geometry" /> bases on the supplied Well-known Binary representation.</returns>
        public static Geometry Parse(byte[] bytes)
        {
            // Create a memory stream using the suppiled byte array.
            using (var ms = new MemoryStream(bytes))
            {
                // Create a new binary reader using the newly created memorystream.
                using (var reader = new BinaryReader(ms))
                {
                    // Call the main create function.
                    return Parse(reader);
                }
            }
        }

        /// <summary>
        ///     Creates a <see cref="Mapsui.Geometries.Geometry" /> based on the Well-known binary representation.
        /// </summary>
        /// <param name="reader">
        ///     A <see cref="System.IO.BinaryReader">BinaryReader</see> used to read the Well-known binary
        ///     representation.
        /// </param>
        /// <returns>A <see cref="Mapsui.Geometries.Geometry" /> based on the Well-known binary representation.</returns>
        public static Geometry Parse(BinaryReader reader)
        {
            // Get the first Byte in the array. This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            var byteOrder = reader.ReadByte();

            // Get the type of this geometry.
            var type = ReadUInt32(reader, (WkbByteOrder) byteOrder);

            switch ((WKBGeometryType) type)
            {
                case WKBGeometryType.WKBPoint:
                    return CreateWKBPoint(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBLineString:
                    return CreateWKBLineString(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBPolygon:
                    return CreateWKBPolygon(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBMultiPoint:
                    return CreateWKBMultiPoint(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBMultiLineString:
                    return CreateWKBMultiLineString(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBMultiPolygon:
                    return CreateWKBMultiPolygon(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.WKBGeometryCollection:
                    return CreateWKBGeometryCollection(reader, (WkbByteOrder) byteOrder);

                default:
                    if (!Enum.IsDefined(typeof(WKBGeometryType), type))
                        throw new ArgumentException("Geometry type not recognized");
                    throw new NotSupportedException("Geometry type '" + type + "' not supported");
            }
        }

        private static Point CreateWKBPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Create and return the point.
            return new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
        }

        private static IEnumerable<Point> ReadCoordinates(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this linestring.
            var numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array of coordinates.
            var coords = new Point[numPoints];

            // Loop on the number of points in the ring.
            for (var i = 0; i < numPoints; i++)
            {
                // Add the coordinate.
                coords[i] = new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
            }
            return coords;
        }

        private static LineString CreateWKBLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            var l = new LineString();
            //l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            var arrPoint = ReadCoordinates(reader, byteOrder);
            foreach (var t in arrPoint)
            {
                l.Vertices.Add(t);
            }

            return l;
        }

        private static LinearRing CreateWKBLinearRing(BinaryReader reader, WkbByteOrder byteOrder)
        {
            var l = new LinearRing();
            //l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            var arrPoint = ReadCoordinates(reader, byteOrder);
            foreach (var t in arrPoint)
            {
                l.Vertices.Add(t);
            }

            //if polygon isn't closed, add the first point to the end (this shouldn't occur for correct WKB data)
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((l.Vertices[0].X != l.Vertices[l.Vertices.Count - 1].X) ||
                (l.Vertices[0].Y != l.Vertices[l.Vertices.Count - 1].Y))
                l.Vertices.Add(new Point(l.Vertices[0].X, l.Vertices[0].Y));
            // ReSharper restore CompareOfFloatsByEqualityOperator
            return l;
        }

        private static Polygon CreateWKBPolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the Number of rings in this Polygon.
            var numRings = (int) ReadUInt32(reader, byteOrder);

            Debug.Assert(numRings >= 1, "Number of rings in polygon must be 1 or more.");

            var shell = new Polygon(CreateWKBLinearRing(reader, byteOrder));

            // Create a new array of linearrings for the interior rings.
            for (var i = 0; i < numRings - 1; i++)
            {
                shell.InteriorRings.Add(CreateWKBLinearRing(reader, byteOrder));
            }

            // Create and return the Poylgon.
            return shell;
        }

        private static MultiPoint CreateWKBMultiPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this multipoint.
            var numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the points.
            var points = new MultiPoint();

            // Loop on the number of points.
            for (var i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next point and add it to the point array.
                points.Points.Add(CreateWKBPoint(reader, byteOrder));
            }
            return points;
        }

        private static MultiLineString CreateWKBMultiLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of linestrings in this multilinestring.
            var numLineStrings = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            var mline = new MultiLineString();

            // Loop on the number of linestrings.
            for (var i = 0; i < numLineStrings; i++)
            {
                // Read linestring header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // Create the next linestring and add it to the array.
                mline.LineStrings.Add(CreateWKBLineString(reader, byteOrder));
            }

            // Create and return the MultiLineString.
            return mline;
        }

        private static MultiPolygon CreateWKBMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of Polygons.
            var numPolygons = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            var polygons = new MultiPolygon();

            // Loop on the number of polygons.
            for (var i = 0; i < numPolygons; i++)
            {
                // read polygon header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next polygon and add it to the array.
                polygons.Polygons.Add(CreateWKBPolygon(reader, byteOrder));
            }

            //Create and return the MultiPolygon.
            return polygons;
        }

        private static Geometry CreateWKBGeometryCollection(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // The next byte in the array tells the number of geometries in this collection.
            var numGeometries = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the geometries.
            var geometries = new GeometryCollection();

            // Loop on the number of geometries.
            for (var i = 0; i < numGeometries; i++)
            {
                // Call the main create function with the next geometry.
                geometries.Collection.Add(Parse(reader));
            }

            // Create and return the next geometry.
            return geometries;
        }

        private static uint ReadUInt32(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                var bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadUInt32();
            throw new ArgumentException("Byte order not recognized");
        }

        private static double ReadDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                var bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }

            if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadDouble();

            throw new ArgumentException("Byte order not recognized");
        }
    }
}