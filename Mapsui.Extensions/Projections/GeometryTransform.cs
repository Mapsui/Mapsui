using System;
using DotSpatial.Projections;

namespace Mapsui.Nts.Projections;

public sealed class GeometryTransform : NetTopologySuite.Geometries.ICoordinateSequenceFilter
{
    private readonly (ProjectionInfo? From, ProjectionInfo? To) _transform;

    public GeometryTransform((ProjectionInfo? From, ProjectionInfo? To) transform)
    {
        _transform = transform;
    }

    public bool Done => false;

    public bool GeometryChanged => true;

    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i)
    {
        var x = seq.GetX(i);
        var y = seq.GetY(i);
        var z = seq.GetZ(i);
        var pointsXy = new[] { x, y };
        var pointsZ = new[] { z };
        Reproject.ReprojectPoints(pointsXy, pointsZ, _transform.From, _transform.To, 0, 1);
        seq.SetX(i, pointsXy[0]);
        seq.SetY(i, pointsXy[1]);
        seq.SetZ(i, pointsZ[0]);
    }
}
