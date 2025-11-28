using System;
using NetTopologySuite.IO.VectorTiles;

namespace Mapsui.Experimental.VectorTiles.Extensions;

public static class VectorTileExtensions
{
    /// <summary>
    /// Creates a shallow copy of the specified <see cref="VectorTile"/>.
    /// Copies the <see cref="VectorTile.TileId"/> and references to the existing layers.
    /// </summary>
    /// <param name="tile">The source tile.</param>
    /// <returns>A new <see cref="VectorTile"/> instance with the same <see cref="VectorTile.TileId"/> and layers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tile"/> is null.</exception>
    public static VectorTile Copy(this VectorTile tile)
    {
        var copy = new VectorTile { TileId = tile.TileId };
        // Shallow copy of layers: maintain references to existing Layer instances
        foreach (var layer in tile.Layers)
        {
            copy.Layers.Add(layer);
        }
        return copy;
    }
}
