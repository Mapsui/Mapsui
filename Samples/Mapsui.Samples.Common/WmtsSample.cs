using System.Linq;
using System.Net;
using BruTile.Extensions;
using BruTile.Wmts;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    public static class WmtsSample
    {
        public static ILayer CreateLayer()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("http://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities");
            var webResponse = webRequest.GetSyncResponse(10000);
            if (webResponse == null) throw (new WebException("An error occurred while fetching tile", null));
            using (var responseStream = webResponse.GetResponseStream())
            {
                var tileSources = WmtsParser.Parse(responseStream);
                var natura2000 = tileSources.First(t => t.Name.ToLower().Contains("natura2000"));
                return new TileLayer(natura2000) { Name = "Natura 2000" };
            }
        }
    }
}
