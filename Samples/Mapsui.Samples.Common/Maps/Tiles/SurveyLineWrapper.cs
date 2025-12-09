using Mapsui.Nts;
using MarinerNotices.MapsuiBuilder.Enums;
using System;

namespace MarinerNotices.MapsuiBuilder.Wrappers;

public class SurveyLineWrapper : BaseWrapper
{
    public SurveyLineWrapper(GeometryFeature feature, ProjectGeometryZoneType zoneType = ProjectGeometryZoneType.MainArea)
        : base(feature)
    {
        ZoneType = zoneType;
    }

    public int Color => Convert.ToInt32(Feature["color"]);

    public ProjectGeometryZoneType ZoneType { get; init; }

    public override string GetSymbolStyleKey()
    {
        // Return the _instanceKey if you want to work with random colors: return _instanceKey;
        return $"{FeatureType}_{Type}";
    }
}
