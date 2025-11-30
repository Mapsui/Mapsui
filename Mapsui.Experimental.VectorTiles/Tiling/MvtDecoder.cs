using Mapsui.Logging;
using NetTopologySuite.IO.VectorTiles;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using System;
using System.IO;

namespace Mapsui.Experimental.VectorTiles.Tiling;

public static class MvtDecoder
{
    /// <summary>
    /// Decodes a Mapbox Vector Tile (MVT) from a byte array.
    /// </summary>
    /// <param name="tileData">The MVT tile data as a byte array.</param>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <param name="zoom">The zoom level.</param>
    /// <returns>A dictionary where keys are layer names and values are lists of features in that layer.</returns>
    public static VectorTile DecodeTile(byte[] tileData, int x, int y, int zoom)
    {
        if (tileData.Length == 0)
            return new VectorTile();

        try
        {
            // Create a tile definition
            var tileDefinition = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom);

            // Read the vector tile from the byte array
            using var stream = new MemoryStream(tileData);
            return new MapboxTileReader().Read(stream, tileDefinition);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"An error occurred while decoding the vector tile x: {x}, y: {y}, zoom: {zoom}", ex);
            return new VectorTile();
        }
    }
}
