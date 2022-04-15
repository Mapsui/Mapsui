using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common;

public abstract class AsyncSampleBase : IAsyncSample
{
    public abstract string Name { get; }
    public abstract string Category { get; }
    public abstract Task SetupAsync(IMapControl mapControl);
}