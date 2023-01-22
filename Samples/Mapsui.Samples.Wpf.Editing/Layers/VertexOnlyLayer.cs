using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Wpf.Editing.Layers;

public class VertexOnlyLayer : BaseLayer
{
    private readonly WritableLayer _source;

    public override MRect? Extent => _source.Extent;

    public VertexOnlyLayer(WritableLayer source)
    {
        _source = source;
        _source.DataChanged += (_, args) => OnDataChanged(args);
        Style = new SymbolStyle { SymbolScale = 0.5 };
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        var features = _source.GetFeatures(box, resolution).Cast<GeometryFeature>().ToList();
        foreach (var feature in features)
        {
            if (feature.Geometry is Point || feature.Geometry is MultiPoint) continue; // Points with a vertex on top confuse me
            if (feature.Geometry != null)
                foreach (var vertex in feature.Geometry.MainCoordinates())
                {
                    yield return new GeometryFeature { Geometry = new Point(vertex) };
                }
        }
    }
}
