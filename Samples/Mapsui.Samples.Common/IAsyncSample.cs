using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common;

public interface IAsyncSample : ISampleBase
{
    Task SetupAsync(IMapControl mapControl);
}