using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common;

public interface ISample : ISampleBase
{
    Task<Map> CreateMapAsync();
}