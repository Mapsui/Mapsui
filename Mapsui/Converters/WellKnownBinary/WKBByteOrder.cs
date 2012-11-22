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

namespace SharpMap.Converters.WellKnownBinary
{
    /// <summary>
    /// Specifies the specific binary encoding (NDR or XDR) used for a geometry byte stream
    /// </summary>
    public enum WkbByteOrder : byte
    {
        /// <summary>
        /// XDR (Big Endian) Encoding of Numeric Types
        /// </summary>
        /// <remarks>
        /// <para>The XDR representation of an Unsigned Integer is Big Endian (most significant byte first).</para>
        /// <para>The XDR representation of a Double is Big Endian (sign bit is first byte).</para>
        /// </remarks>
        Xdr = 0,
        /// <summary>
        /// NDR (Little Endian) Encoding of Numeric Types
        /// </summary>
        /// <remarks>
        /// <para>The NDR representation of an Unsigned Integer is Little Endian (least significant byte first).</para>
        /// <para>The NDR representation of a Double is Little Endian (sign bit is last byte).</para>
        /// </remarks>
        Ndr = 1
    }
}