using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using BruTile.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.Providers.ArcGIS.Dynamic
{
    //Documentation 9.3: http://resources.esri.com/help/9.3/arcgisserver/apis/rest/
    //Documentation 10.0: http://help.arcgis.com/EN/arcgisserver/10.0/apis/rest/index.html
    public delegate void StatusEventHandler(object sender, ArcGISFeatureInfo featureInfo);

    public class ArcGISIdentify
    {
        private int _timeOut { get; set; }
        private HttpWebRequest _webRequest { get; set; }
        private ArcGISFeatureInfo _featureInfo { get; set; }

        public event StatusEventHandler IdentifyFinished;
        public event StatusEventHandler IdentifyFailed;

        public ArcGISIdentify()
        {
            TimeOut = 5000;
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 5 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// Request a ArcGIS Service for FeatureInfo
        /// </summary>
        /// <param name="url">Mapserver url</param>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="tolerance">The distance in screen pixels from the specified geometry within which the identify should be performed</param>
        /// <param name="layers">The layers to perform the identify operation on</param>
        /// <param name="extendXmin">The extent or bounding box of the map currently being viewed.</param>
        /// <param name="extendYmin">The extent or bounding box of the map currently being viewed.</param>
        /// <param name="extendXmax">The extent or bounding box of the map currently being viewed.</param>
        /// <param name="extendYmax">The extent or bounding box of the map currently being viewed.</param>
        /// <param name="mapWidth">The screen image display width</param>
        /// <param name="mapHeight">The screen image display height</param>
        /// <param name="mapDpi">The screen image display dpi, default is: 96</param>
        /// <param name="returnGeometry"></param>
        /// <param name="credentials"></param>
        /// <param name="sr">sr code of input geometry</param>
        public void Request(string url, double x, double y, int tolerance, string[] layers, double extendXmin, double extendYmin, double extendXmax, double extendYmax, double mapWidth, double mapHeight, double mapDpi, bool returnGeometry, ICredentials credentials = null, int sr = int.MinValue)
        {
            //remove trailing slash from url
            if (url.Length > 0 && url[url.Length - 1].Equals('/'))
                url = url.Remove(url.Length - 1, 1);

            var pointGeom = string.Format(CultureInfo.InvariantCulture, "{0},{1}", x, y);
            var layersString = CreateLayersString(layers);
            var mapExtend = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", extendXmin, extendYmin, extendXmax, extendYmax);
            var imageDisplay = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", mapWidth, mapHeight, mapDpi);
            var requestUrl = string.Format("{0}/identify?f=pjson&geometryType=esriGeometryPoint&geometry={1}&tolerance={2}{3}&mapExtent={4}&imageDisplay={5}&returnGeometry={6}{7}", url, pointGeom, tolerance, layersString, mapExtend, imageDisplay, returnGeometry, sr != int.MinValue ? string.Format("&sr={0}", sr) : "");

            _webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            if (credentials == null)
                _webRequest.UseDefaultCredentials = true;
            else
                _webRequest.Credentials = credentials;

            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        private static string CreateLayersString(IList<string> layers)
        {
            if (layers.Count == 0)//if no layers defined request all layers
                return "";

            var layerString = "&layers=all:";

            for (var i = 0; i < layers.Count; i++)
            {
                layerString += layers[i];

                if (i + 1 < layers.Count)
                    layerString += ",";
            }

            return layerString;
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = _webRequest.GetSyncResponse(_timeOut);
                var dataStream = CopyAndClose(response.GetResponseStream());

                if (dataStream != null)
                {
                    var sReader = new StreamReader(dataStream);
                    var jsonString = sReader.ReadToEnd();

                    var serializer = new JsonSerializer();
                    var jToken = JObject.Parse(jsonString);
                    _featureInfo = (ArcGISFeatureInfo)serializer.Deserialize(new JTokenReader(jToken), typeof(ArcGISFeatureInfo));

                    dataStream.Position = 0;

                    using (var reader = new StreamReader(dataStream))
                    {
                        var contentString = reader.ReadToEnd();
                        if (contentString.Contains("{\"error\":{\""))
                        {
                            OnIdentifyFailed();
                            return;
                        }
                    }
                    dataStream.Dispose();
                }

                response.Dispose();
                _webRequest.EndGetResponse(result);
                OnIdentifyFinished();
            }
            catch (WebException)
            {
                OnIdentifyFailed();
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

        private void OnIdentifyFinished()
        {
            var handler = IdentifyFinished;
            if (handler != null) handler(this, _featureInfo);
        }

        private void OnIdentifyFailed()
        {
            var handler = IdentifyFailed;
            if (handler != null) handler(this, null);
        }
    }
}
