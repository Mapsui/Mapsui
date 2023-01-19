using System.Threading.Tasks;

namespace Mapsui.Samples.Common;

public interface ISample : ISampleBase
{
    Task<Map> CreateMapAsync();
}
