using Mapsui;
using Mapsui.Nts;
using MarinerNotices.MapsuiBuilder.Functions;
using NetTopologySuite.Geometries;
using System;

namespace MarinerNotices.MapsuiBuilder.Wrappers;

public abstract class BaseWrapper //!!!: IProjectInfo, ICalloutInfo
{
    private readonly GeometryFeature _feature;
    private string? _cachedTruncatedDescription; // Store the truncated description so we don't have to recalculate it.

    public BaseWrapper(GeometryFeature feature)
    {
        _feature = feature;
    }

    public IFeature Feature => _feature;

    public string Title => Feature["name"]?.ToString() ?? string.Empty;

    public string Description => Feature["description"]?.ToString() ?? string.Empty;

    public string TruncatedDescription => _cachedTruncatedDescription ??=
        StringUtilities.TruncateDescription(Description, 64);

    public string FeatureType => Feature["featureType"]?.ToString() ?? throw new Exception("The featureType was not set.");

    public Geometry CalloutGeometry => _feature.Geometry!;

    public int ProjectId => Convert.ToInt32(Feature["projectId"]);

    public string UniqueIdentifier => _feature["featureId"]?.ToString() ?? string.Empty;

    public int Type => Convert.ToInt32(Feature["type"] ?? -1);

    public int ClusterSize => Convert.ToInt32(Feature["clusterSize"] ?? -1);

    public int Status => Convert.ToInt32(Feature["status"] ?? -1);

    protected static MPoint ToMPoint(Geometry geometry)
    {
        var point = (Point)geometry; // We only accept points at the moment.
        return new MPoint(point.X, point.Y);
    }

    // To improve: Move this method to the style builder because only the style builder knows which fields are used to create the style.
    public abstract string GetSymbolStyleKey();
}
