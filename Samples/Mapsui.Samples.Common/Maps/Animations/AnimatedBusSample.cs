using System.Threading.Tasks;
using Mapsui.Samples.Common.Maps.Geometries;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Animations;

public class AnimatedBusSample : ISample
{
    public string Name => "Animated Bus";

    public string Category => "Animations";

    public Task<Map> CreateMapAsync()
    {
        return new ManyMutatingLayers().CreateMapAsync();
    }
}
