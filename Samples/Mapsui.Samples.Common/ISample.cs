using Mapsui.UI;

namespace Mapsui.Samples.Common
{
    public interface ISample
    {
        string Name { get; }
        string Category { get; }
        void Setup(IMapControl mapControl);
    }
}
