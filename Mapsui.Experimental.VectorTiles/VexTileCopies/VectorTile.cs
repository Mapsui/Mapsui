using System.Collections.Generic;
using VexTile.Renderer.Mvt.AliFlux.Drawing;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VectorTile
{
    public List<VectorTileLayer> Layers = new();

    public bool IsOverZoomed { get; set; }

    /// <summary>
    /// Transforms geometry coordinates from the given source extent to [0..feature.Extent]
    /// by mutating in place. Safe to call because tiles are always freshly decoded from PBF
    /// bytes and are not shared or cached.
    /// </summary>
    public void ApplyExtentInPlace(Rect extent)
    {
        foreach (var layer in Layers)
        {
            foreach (var feature in layer.Features)
            {
                var featureExtent = feature.Extent;
                foreach (var ring in feature.Geometry)
                {
                    for (var i = 0; i < ring.Count; i++)
                    {
                        var p = ring[i];
                        ring[i] = new Point(
                            Utils.ConvertRange(p.X, extent.Left, extent.Right, 0.0, featureExtent),
                            Utils.ConvertRange(p.Y, extent.Top, extent.Bottom, 0.0, featureExtent));
                    }
                }
            }
        }
    }
}
