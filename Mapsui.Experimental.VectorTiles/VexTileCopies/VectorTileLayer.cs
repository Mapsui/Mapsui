using System.Collections.Generic;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VectorTileLayer
{
    public string? Name { get; set; }

    public List<VectorTileFeature> Features { get; } = new List<VectorTileFeature>();
}
