using Mapsui.Nts;
using MarinerNotices.MapsuiBuilder.Enums;

namespace MarinerNotices.MapsuiBuilder.Wrappers;

public class ProjectCenterWrapper : BaseWrapper
{
    public ProjectCenterWrapper(GeometryFeature feature, ProjectGeometryZoneType zoneType = ProjectGeometryZoneType.MainArea)
        : base(feature)
    {
    }

    public override string GetSymbolStyleKey()
    {
        return $"{FeatureType}_{Type}_{ClusterSize}";
    }
}
