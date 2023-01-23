using System;
using System.Diagnostics;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts;

public class GeometryFeature : BaseFeature, IFeature, IDisposable
{
    private bool _disposed;

    public GeometryFeature()
    {
    }

    public GeometryFeature(GeometryFeature geometryFeature) : base(geometryFeature)
    {
        Geometry = geometryFeature.Geometry?.Copy();
    }

    public GeometryFeature(Geometry? geometry)
    {
        Geometry = geometry;
    }

    public Geometry? Geometry { get; set; }

    public MRect? Extent => Geometry?.EnvelopeInternal.ToMRect(); // Todo: Make not-nullable

    public override void Dispose()
    {
        if (_disposed) return;
        base.Dispose();

        foreach (var keyValuePair in RenderedGeometry)
        {
            var disposable = keyValuePair.Value as IDisposable;
            disposable?.Dispose();
        }
        RenderedGeometry.Clear();

        _disposed = true;
    }

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
