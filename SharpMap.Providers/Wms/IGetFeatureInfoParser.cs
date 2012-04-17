using System.IO;

namespace SharpMap.Providers.Wms
{
    public interface IGetFeatureInfoParser
    {
        FeatureInfo ParseWMSResult(string layerName, Stream result);
    }
}
