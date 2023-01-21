using System.IO;

namespace Mapsui.Providers.Wms;

public interface IGetFeatureInfoParser
{
    FeatureInfo ParseWMSResult(string? layerName, Stream result);
}
