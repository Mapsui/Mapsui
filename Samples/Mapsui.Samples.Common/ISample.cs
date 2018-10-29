using Mapsui.UI;

namespace Mapsui.Samples.Common
{
    public interface ISample
    {
        string Name { get; }
        void Setup(IMapControl mapControl);
    }

    public interface IDemoSample : ISample
    {

    }
}
