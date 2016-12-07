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
using System.IO;

namespace Mapsui.Geometries.WellKnownBinary
{
    /// <summary>
    ///     Converts a <see cref="Mapsui.Geometries.Geometry" /> instance to a Well-known Binary string representation.
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
    public static class GeometryToWKB
    {
        //private const byte WKBByteOrder = 0;

        /// <summary>
        ///     Writes a geometry to a byte array using the specified encoding.
        /// </summary>
        /// <param name="g">The geometry to write</param>
        /// <param name="wkbByteOrder">Byte order</param>
        /// <returns>WKB representation of the geometry</returns>
        public static byte[] Write(Geometry g, WkbByteOrder wkbByteOrder = WkbByteOrder.Ndr)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            //Write the byteorder format.
            bw.Write((byte) wkbByteOrder);

            //Write the type of this geometry
            WriteType(g, bw, wkbByteOrder);

            //Write the geometry
            WriteGeometry(g, bw, wkbByteOrder);

            return ms.ToArray();
        }

        /// <summary>
        ///     Writes an unsigned integer to the binarywriter using the specified encoding
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="writer">Binary Writer</param>
        /// <param name="byteOrder">byteorder</param>
        private static void WriteUInt32(uint value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToUInt32(bytes, 0));
            }
            else
                writer.Write(value);
        }

        /// <summary>
        ///     Writes a double to the binarywriter using the specified encoding
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="writer">Binary Writer</param>
        /// <param name="byteOrder">byteorder</param>
        private static void WriteDouble(double value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToDouble(bytes, 0));
            }
            else
                writer.Write(value);
        }

        /// <summary>
        ///     Writes the type number for this geometry.
        /// </summary>
        /// <param name="geometry">The geometry to determine the type of.</param>
        /// <param name="bWriter">Binary Writer</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteType(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Determine the type of the geometry.
            switch (geometry.GetType().FullName)
            {
                //Points are type 1.
                case "Mapsui.Geometries.Point":
                    WriteUInt32((uint) WKBGeometryType.WKBPoint, bWriter, byteorder);
                    break;
                //Linestrings are type 2.
                case "Mapsui.Geometries.LineString":
                    WriteUInt32((uint) WKBGeometryType.WKBLineString, bWriter, byteorder);
                    break;
                //Polygons are type 3.
                case "Mapsui.Geometries.Polygon":
                    WriteUInt32((uint) WKBGeometryType.WKBPolygon, bWriter, byteorder);
                    break;
                //Mulitpoints are type 4.
                case "Mapsui.Geometries.MultiPoint":
                    WriteUInt32((uint) WKBGeometryType.WKBMultiPoint, bWriter, byteorder);
                    break;
                //Multilinestrings are type 5.
                case "Mapsui.Geometries.MultiLineString":
                    WriteUInt32((uint) WKBGeometryType.WKBMultiLineString, bWriter, byteorder);
                    break;
                //Multipolygons are type 6.
                case "Mapsui.Geometries.MultiPolygon":
                    WriteUInt32((uint) WKBGeometryType.WKBMultiPolygon, bWriter, byteorder);
                    break;
                //Geometrycollections are type 7.
                case "Mapsui.Geometries.GeometryCollection":
                    WriteUInt32((uint) WKBGeometryType.WKBGeometryCollection, bWriter, byteorder);
                    break;
                //If the type is not of the above 7 throw an exception.
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        ///     Writes the geometry to the binary writer.
        /// </summary>
        /// <param name="geometry">The geometry to be written.</param>
        /// <param name="bWriter"></param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteGeometry(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            switch (geometry.GetType().FullName)
            {
                //Write the point.
                case "Mapsui.Geometries.Point":
                    WritePoint((Point) geometry, bWriter, byteorder);
                    break;
                case "Mapsui.Geometries.LineString":
                    var ls = (LineString) geometry;
                    WriteLineString(ls, bWriter, byteorder);
                    break;
                case "Mapsui.Geometries.Polygon":
                    WritePolygon((Polygon) geometry, bWriter, byteorder);
                    break;
                //Write the Multipoint.
                case "Mapsui.Geometries.MultiPoint":
                    WriteMultiPoint((MultiPoint) geometry, bWriter, byteorder);
                    break;
                //Write the Multilinestring.
                case "Mapsui.Geometries.MultiLineString":
                    WriteMultiLineString((MultiLineString) geometry, bWriter, byteorder);
                    break;
                //Write the Multipolygon.
                case "Mapsui.Geometries.MultiPolygon":
                    WriteMultiPolygon((MultiPolygon) geometry, bWriter, byteorder);
                    break;
                //Write the Geometrycollection.
                case "Mapsui.Geometries.GeometryCollection":
                    WriteGeometryCollection((GeometryCollection) geometry, bWriter, byteorder);
                    break;
                //If the type is not of the above 7 throw an exception.
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        ///     Writes a point.
        /// </summary>
        /// <param name="point">The point to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WritePoint(Point point, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the x coordinate.
            WriteDouble(point.X, bWriter, byteorder);
            //Write the y coordinate.
            WriteDouble(point.Y, bWriter, byteorder);
        }

        /// <summary>
        ///     Writes a linestring.
        /// </summary>
        /// <param name="ls">The linestring to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteLineString(LineString ls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of points in this linestring.
            WriteUInt32((uint) ls.Vertices.Count, bWriter, byteorder);

            //Loop on each vertices.
            foreach (var p in ls.Vertices)
            {
                WritePoint(p, bWriter, byteorder);
            }
        }

        /// <summary>
        ///     Writes a polygon.
        /// </summary>
        /// <param name="poly">The polygon to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WritePolygon(Polygon poly, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Get the number of rings in this polygon.
            var numRings = poly.InteriorRings.Count + 1;

            //Write the number of rings to the stream (add one for the shell)
            WriteUInt32((uint) numRings, bWriter, byteorder);

            //Write the exterior of this polygon.
            WriteLineString(poly.ExteriorRing, bWriter, byteorder);

            //Loop on the number of rings - 1 because we already wrote the shell.
            foreach (var lr in poly.InteriorRings)
                //Write the (lineString)LinearRing.
            {
                WriteLineString(lr, bWriter, byteorder);
            }
        }

        /// <summary>
        ///     Writes a multipoint.
        /// </summary>
        /// <param name="mp">The multipoint to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiPoint(MultiPoint mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of points.
            WriteUInt32((uint) mp.Points.Count, bWriter, byteorder);

            //Loop on the number of points.
            foreach (var p in mp.Points)
            {
                //Write Points Header
                bWriter.Write((byte) byteorder);
                WriteUInt32((uint) WKBGeometryType.WKBPoint, bWriter, byteorder);
                //Write each point.
                WritePoint(p, bWriter, byteorder);
            }
        }

        /// <summary>
        ///     Writes a multilinestring.
        /// </summary>
        /// <param name="mls">The multilinestring to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiLineString(MultiLineString mls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of linestrings.
            WriteUInt32((uint) mls.LineStrings.Count, bWriter, byteorder);

            //Loop on the number of linestrings.
            foreach (var ls in mls.LineStrings)
            {
                //Write LineString Header
                bWriter.Write((byte) byteorder);
                WriteUInt32((uint) WKBGeometryType.WKBLineString, bWriter, byteorder);
                //Write each linestring.
                WriteLineString(ls, bWriter, byteorder);
            }
        }

        /// <summary>
        ///     Writes a multipolygon.
        /// </summary>
        /// <param name="mp">The mulitpolygon to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteMultiPolygon(MultiPolygon mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of polygons.
            WriteUInt32((uint) mp.Polygons.Count, bWriter, byteorder);

            //Loop on the number of polygons.
            foreach (var poly in mp.Polygons)
            {
                //Write polygon header
                bWriter.Write((byte) byteorder);
                WriteUInt32((uint) WKBGeometryType.WKBPolygon, bWriter, byteorder);
                //Write each polygon.
                WritePolygon(poly, bWriter, byteorder);
            }
        }

        /// <summary>
        ///     Writes a geometrycollection.
        /// </summary>
        /// <param name="gc">The geometrycollection to be written.</param>
        /// <param name="bWriter">Stream to write to.</param>
        /// <param name="byteorder">Byte order</param>
        private static void WriteGeometryCollection(GeometryCollection gc, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Get the number of geometries in this geometrycollection.
            var numGeometries = gc.NumGeometries;

            //Write the number of geometries.
            WriteUInt32((uint) numGeometries, bWriter, byteorder);

            //Loop on the number of geometries.
            for (var i = 0; i < numGeometries; i++)
            {
                //Write the byte-order format of the following geometry.
                bWriter.Write((byte) byteorder);
                //Write the type of each geometry.
                WriteType(gc[i], bWriter, byteorder);
                //Write each geometry.
                WriteGeometry(gc[i], bWriter, byteorder);
            }
        }
    }
}