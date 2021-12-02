using Mapsui.Layers;

namespace Mapsui.Extensions
{
    public static class FetchInfoExtensions
    {
        public static Viewport? ToViewport(this FetchInfo fetchInfo)
        {
            if (fetchInfo.Extent == null)
                return null;
            return Viewport.Create(fetchInfo.Extent, fetchInfo.Resolution);
        }
    }
}
