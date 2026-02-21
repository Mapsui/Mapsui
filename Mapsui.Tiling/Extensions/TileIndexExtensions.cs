// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using BruTile;

namespace Mapsui.Tiling.Extensions;

/// <summary>
/// Extension methods for <see cref="TileIndex"/>.
/// </summary>
public static class TileIndexExtensions
{
    private const int _rowBits = 28;
    private const int _colBits = 28;
    private const int _levelBits = 8;
    private const long _rowMask = (1L << _rowBits) - 1;    // 0x0FFFFFFF
    private const long _colMask = (1L << _colBits) - 1;    // 0x0FFFFFFF
    private const long _levelMask = (1L << _levelBits) - 1; // 0xFF

    /// <summary>
    /// Encodes a <see cref="TileIndex"/> into a single <see cref="long"/> value.
    /// This is useful for using tile indices as dictionary keys or cache identifiers.
    /// </summary>
    /// <remarks>
    /// Bit layout (64 bits total):
    /// <list type="bullet">
    ///   <item>Bits 0-27 (28 bits): Row - supports up to 268 million tiles per axis</item>
    ///   <item>Bits 28-55 (28 bits): Col - supports up to 268 million tiles per axis</item>
    ///   <item>Bits 56-63 (8 bits): Level - supports zoom levels 0-255</item>
    /// </list>
    /// This encoding supports tile coordinates far beyond any practical map zoom level
    /// (web maps typically max out around level 22-24).
    /// </remarks>
    /// <param name="index">The tile index to encode.</param>
    /// <returns>A long value uniquely representing the tile index.</returns>
    public static long ToLong(this TileIndex index) =>
        ((long)index.Level << (_rowBits + _colBits)) |
        ((index.Col & _colMask) << _rowBits) |
        (index.Row & _rowMask);

    /// <summary>
    /// Decodes a <see cref="long"/> value back into a <see cref="TileIndex"/>.
    /// This is the inverse of <see cref="ToLong"/>.
    /// </summary>
    /// <param name="value">The encoded tile index value.</param>
    /// <returns>The decoded tile index.</returns>
    public static TileIndex ToTileIndex(this long value) =>
        new(
            col: (int)((value >> _rowBits) & _colMask),
            row: (int)(value & _rowMask),
            level: (int)((value >> (_rowBits + _colBits)) & _levelMask));
}
