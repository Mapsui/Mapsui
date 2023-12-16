using System;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts;

public class GeometryFeature : BaseFeature, IFeature
{
    public GeometryFeature()
    {
    }

    public GeometryFeature(long id) : base(id)
    {
    }

    public GeometryFeature(GeometryFeature geometryFeature) : base(geometryFeature)
    {
        Geometry = geometryFeature.Geometry?.Copy();
    }

    public GeometryFeature(GeometryFeature geometryFeature, long id) : base(geometryFeature, id)
    {
        Geometry = geometryFeature.Geometry?.Copy();
    }

    public GeometryFeature(Geometry? geometry)
    {
        Geometry = geometry;
    }

    public Geometry? Geometry { get; set; }

    public MRect? Extent => Geometry?.EnvelopeInternal.ToMRect();

    public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        if (Geometry is null) return;
        var vertices = Geometry.Coordinates;
        foreach (var vertex in vertices)
            visit(vertex.X, vertex.Y, (x, y) =>
            {
                vertex.X = x;
                vertex.Y = y;
            });

        // Recalculate Geometry Values (for example in Polygons).
        Geometry.GeometryChanged();
        // Recalculate the Envelope
        Geometry.GeometryChangedAction();
    }
}
