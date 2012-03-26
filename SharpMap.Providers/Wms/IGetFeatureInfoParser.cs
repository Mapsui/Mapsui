using System.Collections.Generic;
using System.IO;

namespace SharpMap.Providers.Wms
{
    public interface IGetFeatureInfoParser
    {
        List<FeatureInfo> ParseWMSResult(Stream result);
    }
}
