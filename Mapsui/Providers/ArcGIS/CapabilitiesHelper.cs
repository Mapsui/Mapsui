using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using BruTile;
using BruTile.Extensions;
using Mapsui.Providers.ArcGIS.Dynamic;
using Mapsui.Providers.ArcGIS.Image;

namespace Mapsui.Providers.ArcGIS
{
    public enum CapabilitiesType
    {
        ImageServiceCapabilities,
        DynamicServiceCapabilities
    }

    public class CapabilitiesHelper
    {
        #region fields

        private HttpWebRequest _webRequest { get; set; }
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
        /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
        /// <param name="capabilitiesType"></param>
        /// <param name="credentials">Credentials to access the service </param>
        public void GetCapabilities(string url, CapabilitiesType capabilitiesType, ICredentials credentials = null)
        {
            _capabilitiesType = capabilitiesType;
            //Check if user added a trailing slash and remove if exist, some webservers can't handle this
            _url = url;
            if (url[url.Length - 1].Equals('/'))
                _url = url.Remove(url.Length - 1);

            var requestUri = string.Format("{0}?f=json", _url);
            _webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            if (credentials == null)
                _webRequest.UseDefaultCredentials = true;
            else
                _webRequest.Credentials = credentials;

            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = _webRequest.GetSyncResponse(_timeOut);
                var dataStream = CopyAndClose(response.GetResponseStream());

                DataContractJsonSerializer serializer = null;
                if(_capabilitiesType == CapabilitiesType.DynamicServiceCapabilities)
                    serializer = new DataContractJsonSerializer(typeof(ArcGISDynamicCapabilities));
                else if (_capabilitiesType == CapabilitiesType.ImageServiceCapabilities)
                    serializer = new DataContractJsonSerializer(typeof(ArcGISImageCapabilities));

                if (dataStream != null)
                {
                    _arcGisCapabilities = (IArcGISCapabilities)serializer.ReadObject(dataStream);
                    dataStream.Position = 0;
                }
                _arcGisCapabilities.ServiceUrl = _url;

                //Hack because ArcGIS Server doesn't return a normal StatusCode
                if (dataStream != null)
                {
                    using (var reader = new StreamReader(dataStream))
                    {
                        var contentString = reader.ReadToEnd();
                        if (contentString.Contains("{\"error\":{\""))
                        {
                            OnFailed(EventArgs.Empty);
                            return;
                        }
                    }
                }

                if (dataStream != null) dataStream.Dispose();
                response.Dispose();

                _webRequest.EndGetResponse(result);
                OnFinished(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                OnFailed(EventArgs.Empty);
            }
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
            CapabilitiesReceived(_arcGisCapabilities, e);
        }

        protected virtual void OnFailed(EventArgs e)
        {
            CapabilitiesFailed(null, e);
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
                schema.Resolutions[levelId] = new Resolution { Id = levelId, UnitsPerPixel = lod.resolution };
                count++;
            }

            schema.Height = arcGisDynamicCapabilities.tileInfo.cols;
            schema.Width = arcGisDynamicCapabilities.tileInfo.rows;
            schema.Extent = new BruTile.Extent(arcGisDynamicCapabilities.fullExtent.xmin, arcGisDynamicCapabilities.fullExtent.ymin, arcGisDynamicCapabilities.fullExtent.xmax, arcGisDynamicCapabilities.fullExtent.ymax);
            schema.OriginX = arcGisDynamicCapabilities.tileInfo.origin.x;
            schema.OriginY = arcGisDynamicCapabilities.tileInfo.origin.y;

            schema.Name = "ESRI";
            schema.Format = arcGisDynamicCapabilities.tileInfo.format;            
            schema.Axis = AxisDirection.InvertedY;
            schema.Srs = string.Format("EPSG:{0}", arcGisDynamicCapabilities.tileInfo.spatialReference.wkid);

            return schema;
        }
    }
}
