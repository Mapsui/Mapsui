using Mapsui.Nts;
using MarinerNotices.MapsuiBuilder.Enums;
using System;

namespace MarinerNotices.MapsuiBuilder.Wrappers;

public class ProjectPointWrapper : BaseWrapper
{
    public ProjectPointWrapper(GeometryFeature feature, ProjectGeometryZoneType zoneType = ProjectGeometryZoneType.MainArea)
        : base(feature)
    {
        ZoneType = zoneType;
    }

    public int Color => Convert.ToInt32(Feature["color"]);

    public ProjectGeometryZoneType ZoneType { get; init; }

    public override string GetSymbolStyleKey()
    {
        return $"{FeatureType}_{Type}_{Color}";
    }
}
