using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;
using System;

namespace Mapsui.Nts;

/// <summary>
/// Feature representing a NTS geometry on the <cref="Map"/>
/// </summary>
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

    /// <summary>
    /// Geometry for this feature
    /// </summary>
    public Geometry? Geometry { get; set; }

    /// <summary>
    /// Extent of feature
    /// </summary>
    public MRect? Extent => Geometry?.EnvelopeInternal.ToMRect();

    /// <summary>
    /// Order of feature
    /// </summary>
    public int ZOrder { get; set; } = 0;

    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit"></param>
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

    override public void Modified()
    {
        base.Modified();

        // Recalculate Geometry Values (for example in Polygons).
        Geometry?.GeometryChanged();
        // Recalculate the Envelope
        Geometry?.GeometryChangedAction();
    }
}
