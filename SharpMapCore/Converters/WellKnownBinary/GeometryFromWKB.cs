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
using System.Diagnostics;
using System.IO;
using SharpMap.Geometries;

namespace SharpMap.Converters.WellKnownBinary
{
    /// <summary>
    ///  Converts Well-known Binary representations to a <see cref="SharpMap.Geometries.Geometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The Well-known Binary Representation for <see cref="SharpMap.Geometries.Geometry"/> (WKBGeometry) provides a portable 
    /// representation of a <see cref="SharpMap.Geometries.Geometry"/> value as a contiguous stream of bytes. It permits <see cref="SharpMap.Geometries.Geometry"/> 
    /// values to be exchanged between an ODBC client and an SQL database in binary form.</para>
    /// <para>The Well-known Binary Representation for <see cref="SharpMap.Geometries.Geometry"/> is obtained by serializing a <see cref="SharpMap.Geometries.Geometry"/>
    /// instance as a sequence of numeric types drawn from the set {Unsigned Integer, Double} and
    /// then serializing each numeric type as a sequence of bytes using one of two well defined,
    /// standard, binary representations for numeric types (NDR, XDR). The specific binary encoding
    /// (NDR or XDR) used for a geometry byte stream is described by a one byte tag that precedes
    /// the serialized bytes. The only difference between the two encodings of geometry is one of
    /// byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.</para>
    /// </remarks> 
    public class GeometryFromWKB
    {
        /// <summary>
        /// Creates a <see cref="SharpMap.Geometries.Geometry"/> from the supplied byte[] containing the Well-known Binary representation.
        /// </summary>
        /// <param name="bytes">byte[] containing the Well-known Binary representation.</param>
        /// <returns>A <see cref="SharpMap.Geometries.Geometry"/> bases on the supplied Well-known Binary representation.</returns>
        public static Geometry Parse(byte[] bytes)
        {
            // Create a memory stream using the suppiled byte array.
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                // Create a new binary reader using the newly created memorystream.
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // Call the main create function.
                    return Parse(reader);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="SharpMap.Geometries.Geometry"/> based on the Well-known binary representation.
        /// </summary>
        /// <param name="reader">A <see cref="System.IO.BinaryReader">BinaryReader</see> used to read the Well-known binary representation.</param>
        /// <returns>A <see cref="SharpMap.Geometries.Geometry"/> based on the Well-known binary representation.</returns>
        public static Geometry Parse(BinaryReader reader)
        {
            // Get the first Byte in the array. This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            Byte byteOrder = reader.ReadByte();

            // Get the type of this geometry.
            UInt32 type = ReadUInt32(reader, (WkbByteOrder) byteOrder);

            switch ((WKBGeometryType) type)
            {
                case WKBGeometryType.wkbPoint:
                    return CreateWKBPoint(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbLineString:
                    return CreateWKBLineString(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbPolygon:
                    return CreateWKBPolygon(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbMultiPoint:
                    return CreateWKBMultiPoint(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbMultiLineString:
                    return CreateWKBMultiLineString(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbMultiPolygon:
                    return CreateWKBMultiPolygon(reader, (WkbByteOrder) byteOrder);

                case WKBGeometryType.wkbGeometryCollection:
                    return CreateWKBGeometryCollection(reader, (WkbByteOrder) byteOrder);

                default:
                    if (!Enum.IsDefined(typeof (WKBGeometryType), type))
                        throw new ArgumentException("Geometry type not recognized");
                    else
                        throw new NotSupportedException("Geometry type '" + type + "' not supported");
            }
        }

        private static Point CreateWKBPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Create and return the point.
            return new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
        }

        private static Point[] ReadCoordinates(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this linestring.
            int numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array of coordinates.
            Point[] coords = new Point[numPoints];

            // Loop on the number of points in the ring.
            for (int i = 0; i < numPoints; i++)
            {
                // Add the coordinate.
                coords[i] = new Point(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
            }
            return coords;
        }

        private static LineString CreateWKBLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            LineString l = new LineString();
            //l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            Point[] arrPoint = ReadCoordinates(reader, byteOrder);
            for (int i = 0; i < arrPoint.Length; i++)
                l.Vertices.Add(arrPoint[i]);

            return l;
        }

        private static LinearRing CreateWKBLinearRing(BinaryReader reader, WkbByteOrder byteOrder)
        {
            LinearRing l = new LinearRing();
            //l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            Point[] arrPoint = ReadCoordinates(reader, byteOrder);
            for (int i = 0; i < arrPoint.Length; i++)
                l.Vertices.Add(arrPoint[i]);

            //if polygon isn't closed, add the first point to the end (this shouldn't occur for correct WKB data)
            if (l.Vertices[0].X != l.Vertices[l.Vertices.Count - 1].X ||
                l.Vertices[0].Y != l.Vertices[l.Vertices.Count - 1].Y)
                l.Vertices.Add(new Point(l.Vertices[0].X, l.Vertices[0].Y));
            return l;
        }

        private static Polygon CreateWKBPolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the Number of rings in this Polygon.
            int numRings = (int) ReadUInt32(reader, byteOrder);

            Debug.Assert(numRings >= 1, "Number of rings in polygon must be 1 or more.");

            Polygon shell = new Polygon(CreateWKBLinearRing(reader, byteOrder));

            // Create a new array of linearrings for the interior rings.
            for (int i = 0; i < (numRings - 1); i++)
                shell.InteriorRings.Add(CreateWKBLinearRing(reader, byteOrder));

            // Create and return the Poylgon.
            return shell;
        }

        private static MultiPoint CreateWKBMultiPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this multipoint.
            int numPoints = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the points.
            MultiPoint points = new MultiPoint();

            // Loop on the number of points.
            for (int i = 0; i < numPoints; i++)
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
            int numLineStrings = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            MultiLineString mline = new MultiLineString();

            // Loop on the number of linestrings.
            for (int i = 0; i < numLineStrings; i++)
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
            int numPolygons = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            MultiPolygon polygons = new MultiPolygon();

            // Loop on the number of polygons.
            for (int i = 0; i < numPolygons; i++)
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
            int numGeometries = (int) ReadUInt32(reader, byteOrder);

            // Create a new array for the geometries.
            GeometryCollection geometries = new GeometryCollection();

            // Loop on the number of geometries.
            for (int i = 0; i < numGeometries; i++)
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
                byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadUInt32();
            else
                throw new ArgumentException("Byte order not recognized");
        }

        private static double ReadDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
            else if (byteOrder == WkbByteOrder.Ndr)
                return reader.ReadDouble();
            else
                throw new ArgumentException("Byte order not recognized");
        }
    }
}