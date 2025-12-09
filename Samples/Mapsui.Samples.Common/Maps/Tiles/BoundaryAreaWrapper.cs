using Mapsui.Nts;
using MarinerNotices.MapsuiBuilder.Enums;
using System;

namespace MarinerNotices.MapsuiBuilder.Wrappers;

public class BoundaryAreaWrapper : BaseWrapper
{
    public BoundaryAreaWrapper(GeometryFeature feature, ProjectGeometryZoneType zoneType = ProjectGeometryZoneType.MainArea)
        : base(feature)
    {
        ZoneType = zoneType;
    }

    public int Color => Convert.ToInt32(Feature["color"]);

    public ProjectGeometryZoneType ZoneType { get; init; }

    public override string GetSymbolStyleKey()
    {
        return $"{FeatureType}_{Type}_{Status}";
    }
}
