using Mapsui.Nts;

namespace Mapsui.Experimental.VectorTiles.Extensions;

public static class NtsFeatureExtensions
{
    public static IFeature ToMapsui(this NetTopologySuite.Features.IFeature ntsFeature)
    {
        var mapsuiFeature = new GeometryFeature(ntsFeature.Geometry);

        var attributes = ntsFeature.Attributes;
        if (attributes is not null)
        {
            foreach (var name in attributes.GetNames())
            {
                mapsuiFeature[name] = attributes[name];
            }
        }

        return mapsuiFeature;
    }
}
