using System.Collections.Generic;
using VexTile.Renderer.Mvt.AliFlux.Drawing;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public class VectorTile
{
    public List<VectorTileLayer> Layers = new();

    public bool IsOverZoomed { get; set; }

    public VectorTile ApplyExtent(Rect extent)
    {
        VectorTile vectorTile = new VectorTile
        {
            IsOverZoomed = IsOverZoomed
        };
        foreach (VectorTileLayer layer in Layers)
        {
            VectorTileLayer vectorTileLayer = new VectorTileLayer
            {
                Name = layer.Name
            };
            foreach (VectorTileFeature feature in layer.Features)
            {
                VectorTileFeature vectorTileFeature = new VectorTileFeature
                {
                    Attributes = new Dictionary<string, object>(feature.Attributes),
                    Extent = feature.Extent,
                    GeometryType = feature.GeometryType
                };
                List<List<Point>> list = new List<List<Point>>();
                foreach (List<Point> item in feature.Geometry)
                {
                    List<Point> list2 = new List<Point>();
                    foreach (Point item2 in item)
                    {
                        double x = Utils.ConvertRange(item2.X, extent.Left, extent.Right, 0.0, vectorTileFeature.Extent);
                        double y = Utils.ConvertRange(item2.Y, extent.Top, extent.Bottom, 0.0, vectorTileFeature.Extent);
                        list2.Add(new Point(x, y));
                    }
                    list.Add(list2);
                }
                vectorTileFeature.Geometry = list;
                vectorTileLayer.Features.Add(vectorTileFeature);
            }
            vectorTile.Layers.Add(vectorTileLayer);
        }
        return vectorTile;
    }
}
