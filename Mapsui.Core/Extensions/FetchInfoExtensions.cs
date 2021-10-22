using Mapsui.Fetcher;

namespace Mapsui.Extensions
{
    public static class FetchInfoExtensions
    {
        public static Viewport ToViewport(this FetchInfo fetchInfo)
        {
            return new Viewport
            {
                Resolution = fetchInfo.Resolution,
                Center = fetchInfo.Extent.Centroid,
                Width = fetchInfo.Extent.Width / fetchInfo.Resolution,
                Height = fetchInfo.Extent.Height / fetchInfo.Resolution
            };
        }
    }
}
