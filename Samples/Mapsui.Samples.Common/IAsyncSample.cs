using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common;

public interface IAsyncSample : ISample
{
    Task SetupAsync(IMapControl mapControl);
}