using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BruTile;
using BruTile.Extensions;
using Mapsui.Logging;
using Mapsui.Providers.ArcGIS.Dynamic;
using Mapsui.Providers.ArcGIS.Image;
using Newtonsoft.Json;

namespace Mapsui.Providers.ArcGIS
{
    public enum CapabilitiesType
    {
        ImageServiceCapabilities,
        DynamicServiceCapabilities
    }

    public class CapabilitiesHelper
    {
        private IArcGISCapabilities _arcGisCapabilities { get; set; }
        private CapabilitiesType _capabilitiesType;
        private int _timeOut { get; set; }
        private string _url { get; set; }
        public delegate void StatusEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Triggered when finished parsing capabilities, returns Capabilities object
        /// </summary>
        public event StatusEventHandler CapabilitiesReceived;

        /// <summary>
        /// Triggered when failed parsing or getting capabilities
        /// </summary>
        public event StatusEventHandler CapabilitiesFailed;


        /// <summary>
        /// Helper class for getting capabilities of an ArcGIS service + extras
        /// </summary>
        public CapabilitiesHelper()
        {
            TimeOut = 10000;
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 10 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// Get the capabilities of an ArcGIS Map Service
        /// </summary>
        /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
        /// <param name="capabilitiesType"></param>
        public void GetCapabilities(string url, CapabilitiesType capabilitiesType)
        {
            ExecuteRequest(url, capabilitiesType);
        }

        /// <summary>
        /// Get the capabilities of an ArcGIS Map Service
        /// </summary>
        /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
        /// <param name="capabilitiesType"></param>
        /// <param name="token">Token string to access the service </param>
        public void GetCapabilities(string url, CapabilitiesType capabilitiesType, string token = null)
        {
            ExecuteRequest(url, capabilitiesType, null, token);
        }

        /// <summary>
        /// Get the capabilities of an ArcGIS Map Service
        /// </summary>
        /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
        /// <param name="capabilitiesType"></param>
        /// <param name="credentials">Credentials to access the service </param>
        public void GetCapabilities(string url, CapabilitiesType capabilitiesType, ICredentials credentials = null)
        {
            ExecuteRequest(url, capabilitiesType, credentials);
        }

        private void ExecuteRequest(string url, CapabilitiesType capabilitiesType, ICredentials credentials = null, string token = null)
        {
            Task.Run(async () =>
            {
                _capabilitiesType = capabilitiesType;
                _url = RemoveTrailingSlash(url);

                var requestUri = $"{_url}?f=json";
                if (!string.IsNullOrEmpty(token))
                    requestUri = $"{requestUri}&token={token}";

                var handler = new HttpClientHandler { Credentials = credentials ?? CredentialCache.DefaultCredentials };
                var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
                var response = await client.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    OnCapabilitiesFailed(new EventArgs());
                    return;
                }

                try
                {
                    var dataStream = await response.Content.ReadAsStringAsync();

                    if (_capabilitiesType == CapabilitiesType.DynamicServiceCapabilities)
                        _arcGisCapabilities = JsonConvert.DeserializeObject<ArcGISDynamicCapabilities>(dataStream);
                    else if (_capabilitiesType == CapabilitiesType.ImageServiceCapabilities)
                        _arcGisCapabilities = JsonConvert.DeserializeObject<ArcGISImageCapabilities>(dataStream);

                    _arcGisCapabilities.ServiceUrl = _url;

                    //Hack because ArcGIS Server doesn't always return a normal StatusCode
                    if (dataStream.Contains("{\"error\":{\""))
                    {
                        OnCapabilitiesFailed(EventArgs.Empty);
                        return;
                    }

                    OnFinished(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                    OnCapabilitiesFailed(EventArgs.Empty);
                }
            });
        }

        private string RemoveTrailingSlash(string url)
        {
            if (url[url.Length - 1].Equals('/'))
                url = url.Remove(url.Length - 1);

            return url;
        }


        private static Stream CopyAndClose(Stream inputStream)
        {
            const int readSize = 256;
            var buffer = new byte[readSize];
            var ms = new MemoryStream();

            var count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Position = 0;
            inputStream.Dispose();
            return ms;
        }

        protected virtual void OnFinished(EventArgs e)
        {
            CapabilitiesReceived?.Invoke(_arcGisCapabilities, e);
        }

        protected virtual void OnCapabilitiesFailed(EventArgs e)
        {
            CapabilitiesFailed?.Invoke(null, e);
        }

        /// <summary>
        /// Generate BruTile TileSchema based on ArcGIS Capabilities
        /// </summary>
        /// <returns>TileSchema, returns null if service is not tiled</returns>
        public static ITileSchema GetTileSchema(ArcGISDynamicCapabilities arcGisDynamicCapabilities)
        {
            //TODO: Does this belong in Mapsui.Providers?

            if (arcGisDynamicCapabilities.tileInfo == null)
                return null;

            var schema = new TileSchema();
            var count = 0;

            foreach (var lod in arcGisDynamicCapabilities.tileInfo.lods)
            {
                var levelId = count.ToString();
                schema.Resolutions[levelId] = new Resolution(levelId, lod.resolution,
                    arcGisDynamicCapabilities.tileInfo.cols,
                    arcGisDynamicCapabilities.tileInfo.rows);
                count++;
            }

            schema.Extent = new BruTile.Extent(arcGisDynamicCapabilities.fullExtent.xmin, arcGisDynamicCapabilities.fullExtent.ymin, arcGisDynamicCapabilities.fullExtent.xmax, arcGisDynamicCapabilities.fullExtent.ymax);
            schema.OriginX = arcGisDynamicCapabilities.tileInfo.origin.x;
            schema.OriginY = arcGisDynamicCapabilities.tileInfo.origin.y;

            schema.Name = "ESRI";
            schema.Format = arcGisDynamicCapabilities.tileInfo.format;
            schema.YAxis = YAxis.OSM;
            schema.Srs = $"EPSG:{arcGisDynamicCapabilities.tileInfo.spatialReference.wkid}";

            return schema;
        }
    }
}
