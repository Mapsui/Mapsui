using System.IO;

namespace Mapsui.Desktop.Wms
{
    public interface IGetFeatureInfoParser
    {
        FeatureInfo ParseWMSResult(string layerName, Stream result);
    }
}
