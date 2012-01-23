using System;
using BruTile;
using System.Net;
using System.Runtime.Serialization.Json;

namespace SharpMap.Providers.ArcGis
{
    public class CapabilitiesHelper
    {
        #region fields

        private WebRequest _webRequest { get; set; }
        private Capabilities _capabilities { get; set; }
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

        #endregion

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
        /// <param name="url">Url of map service, example: http://url/arcgis/rest/services/test/MapServer</param>
        public void GetCapabilities(string url)
        {
            GetCapabilities(url, CredentialCache.DefaultCredentials);
        }

        /// <summary>
        /// Get the capabilities of an ArcGIS Map Service
        /// </summary>
        /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
        /// <param name="credentials">Credentials to access the service </param>
        public void GetCapabilities(string url, ICredentials credentials)
        {
            //Check if user added a trailing slash and remove if exist, some webservers can't handle this
            _url = url;
            if (url[url.Length - 1].Equals('/'))
                _url = url.Remove(url.Length - 1);

            var requestUri = string.Format("{0}?f=json", _url);
            _webRequest = WebRequest.Create(requestUri);
            _webRequest.Timeout = _timeOut;
            _webRequest.Credentials = credentials;

            _webRequest.BeginGetResponse(new AsyncCallback(FinishWebRequest), null);
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = (HttpWebResponse)_webRequest.GetResponse();
                var dataStream = response.GetResponseStream();

                var serializer = new DataContractJsonSerializer(typeof(Capabilities));
                if (dataStream != null) _capabilities = (Capabilities)serializer.ReadObject(dataStream);
                _capabilities.ServiceUrl = _url;

                if (dataStream != null) dataStream.Close();
                response.Close();

                _webRequest.EndGetResponse(result);
                OnFinished(EventArgs.Empty);
            }
            catch (WebException)
            {
                OnFailed(EventArgs.Empty);
            }
        }

        protected virtual void OnFinished(EventArgs e)
        {
            CapabilitiesReceived(_capabilities, e);
        }

        protected virtual void OnFailed(EventArgs e)
        {
            CapabilitiesFailed(null, e);
        }

        /// <summary>
        /// Generate BruTile TileSchema based on ArcGIS Capabilities
        /// </summary>
        /// <returns>TileSchema, returns null if service is not tiled</returns>
        public static ITileSchema GetTileSchema(Capabilities capabilities)
        {
            //TODO: Does this belong in SharpMap.Providers?

            if (capabilities.tileInfo == null)
                return null;

            var schema = new TileSchema();
            var count = 0;

            foreach (var lod in capabilities.tileInfo.lods)
            {
                schema.Resolutions.Add(new Resolution { Id = count.ToString(), UnitsPerPixel = lod.resolution });
                count++;
            }

            schema.Height = capabilities.tileInfo.cols;
            schema.Width = capabilities.tileInfo.rows;
            schema.Extent = new BruTile.Extent(capabilities.fullExtent.xmin, capabilities.fullExtent.ymin, capabilities.fullExtent.xmax, capabilities.fullExtent.ymax);
            schema.OriginX = capabilities.tileInfo.origin.x;
            schema.OriginY = capabilities.tileInfo.origin.y;
            schema.Name = "ESRI";
            schema.Format = capabilities.tileInfo.format;            
            schema.Axis = AxisDirection.InvertedY;
            schema.Srs = string.Format("EPSG:{0}", capabilities.tileInfo.spatialReference.wkid);

            return schema;
        }
    }
}
