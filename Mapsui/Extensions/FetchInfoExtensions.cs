using Mapsui.Layers;

namespace Mapsui.Extensions;

public static class FetchInfoExtensions
{
    public static Viewport ToViewport(this FetchInfo fetchInfo)
    {
        return Viewport.Create(fetchInfo.Extent, fetchInfo.Resolution);
    }
}
