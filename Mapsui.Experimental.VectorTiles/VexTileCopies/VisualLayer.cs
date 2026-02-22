using System.Collections.Generic;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VisualLayer
{
    public VisualLayerType Type { get; set; }

    public byte[]? RasterData { get; set; }

    public required VectorTileFeature VectorTileFeature { get; set; }

    public required List<List<Point>> Geometry { get; set; }

    public required Brush Brush { get; set; }

    public string Id => $"{LayerId} :: {SourceName} :: {SourceLayer}";

    public required string LayerId { get; set; }

    public required string SourceName { get; set; }

    public required string SourceLayer { get; set; }

    /// <summary>
    /// Insertion order index used as a tiebreaker in sorting to preserve
    /// the stable ordering that the original LINQ OrderBy provides.
    /// </summary>
    public int InsertionOrder { get; set; }
}
