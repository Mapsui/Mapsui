using System.Collections.Generic;
using VexTile.Renderer.Mvt.AliFlux.Drawing;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VectorTileFeature
{
    public double Extent { get; set; }

    public string? GeometryType { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    public List<List<Point>> Geometry { get; set; } = new List<List<Point>>();
}
