using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace SharpMap.Providers.Wms
{
    public delegate void StatusEventHandler(object sender, List<FeatureInfo> featureInfo);

    public class GetFeatureInfo
    {
        private int _timeOut { get; set; }
        private WebRequest _webRequest { get; set; }
        private string _infoFormat;

        public event StatusEventHandler IdentifyFinished;
        public event StatusEventHandler IdentifyFailed;

        public GetFeatureInfo()
        {
            TimeOut = 7000;
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 7 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// Request FeatureInfo for a WMS Server
        /// </summary>
        /// <param name="baseUrl">Base URL of the WMS server</param>
        /// <param name="wmsVersion">WMS Version</param>
        /// <param name="infoFormat">Format of response (text/xml, text/plain, etc)</param>
        /// <param name="srs">EPSG Code of the coordinate system</param>
        /// <param name="layers">Layers to get FeatureInfo From</param>
        /// <param name="extendXmin"></param>
        /// <param name="extendYmin"></param>
        /// <param name="extendXmax"></param>
        /// <param name="extendYmax"></param>
        /// <param name="x">Coordinate in pixels x</param>
        /// <param name="y">Coordinate in pixels y</param>
        /// <param name="mapWidth">Width of the map</param>
        /// <param name="mapHeight">Height of the map</param>
        public void Request(string baseUrl, string wmsVersion, string infoFormat, string srs, string[] layers, double extendXmin, double extendYmin, double extendXmax, double extendYmax, int x, int y, int mapWidth, int mapHeight)
        {
            Request(baseUrl, wmsVersion, infoFormat, srs, layers, extendXmin, extendYmin, extendXmax, extendYmax, x, y, mapWidth, mapHeight, CredentialCache.DefaultCredentials);
        }

        public void Request(string baseUrl, string wmsVersion, string infoFormat, string srs, string[] layers, double extendXmin, double extendYmin, double extendXmax, double extendYmax, int x, int y, int mapWidth, int mapHeight, ICredentials credentials)
        {
            _infoFormat = infoFormat;
            var requestUrl = CreateRequestUrl(baseUrl, wmsVersion, infoFormat, srs, layers, extendXmin, extendYmin, extendXmax, extendYmax, x, y, mapWidth, mapHeight);
            _webRequest = WebRequest.Create(requestUrl);
            _webRequest.Timeout = _timeOut;
            _webRequest.Credentials = credentials;

            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        private static string CreateRequestUrl(string baseUrl, string wmsVersion, string infoFormat, string srs, IList<string> layers, double extendXmin, double extendYmin, double extendXmax, double extendYmax, double x, double y, double mapWidth, double mapHeight)
        {
            //Versions
            var versionNumber = new Version(wmsVersion);
            var version130 = new Version("1.3.0");

            //Set params based on version
            var xParam = versionNumber < version130 ? "X" : "I";
            var yParam = versionNumber < version130 ? "Y" : "J";
            var crsParam = versionNumber < version130 ? "SRS" : "CRS";

            //Create specific strings for the request
            var layerString = CreateLayerString(layers);
            var bboxString = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", extendXmin, extendYmin, extendXmax, extendYmax);

            //Build requist url
            var requestUrl = string.Format(CultureInfo.InvariantCulture,
                                           "{0}{1}SERVICE=WMS&" +
                                           "REQUEST=GetFeatureInfo&" +
                                           "VERSION={2}&" +
                                           "LAYERS={3}&" +
                                           "{4}={5}&" + //SRS
                                           "BBOX={6}&" +
                                           "WIDTH={7}&" +
                                           "HEIGHT={8}&" +
                                           "QUERY_LAYERS={9}&" +
                                           "INFO_FORMAT={10}&" +
                                           "{11}={12}&" +
                                           "{13}={14}&" +
                                           "FEATURE_COUNT=200&" +
                                           "FORMAT=image/png",

                baseUrl, baseUrl.Contains("?") ? "&" : "?", //1 = Prefix
                wmsVersion,
                layerString,
                crsParam,
                srs,
                bboxString,
                mapWidth,
                mapHeight,
                layerString,
                infoFormat,
                xParam, x,
                yParam, y);

            return requestUrl;
        }

        /// <summary>
        /// Create a string of layers to request in a format the WMS expects
        /// </summary>
        private static string CreateLayerString(IList<string> layers)
        {
            var layerString = "";

            for (var i = 0; i < layers.Count; i++)
            {
                layerString += layers[i];
                if (i + 1 < layers.Count)
                    layerString += ",";
            }

            return layerString;
        }

        /// <summary>
        /// Webrequest finished, parse the response
        /// </summary>
        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = (HttpWebResponse)_webRequest.GetResponse();
                var stream = response.GetResponseStream();

                var parser = GetParserFromFormat(_infoFormat);

                //When the output format is currently is not exported
                if (parser == null)
                {
                    response.Close();
                    _webRequest.EndGetResponse(result);
                    OnIdentifyFailed();
                    return;
                }

                var featureInfo = parser.ParseWMSResult(stream);

                response.Close();
                _webRequest.EndGetResponse(result);
                OnIdentifyFinished(featureInfo);
            }
            catch (WebException)
            {
                OnIdentifyFailed();
            }
        }

        /// <summary>
        /// Get a parser that is able to parse the output returned from the service
        /// </summary>
        /// <param name="format">Output format of the service</param>
        private static IGetFeatureInfoParser GetParserFromFormat(string format)
        {
            if (format.Equals("application/vnd.ogc.gml"))
                return new GmlGetFeatureInfoParser();
            if (format.Equals("text/xml; subtype=gml/3.1.1"))
                return new GmlGetFeatureInfoParser();
            if (format.Equals("text/xml"))
                return new XmlGetFeatureInfoParser();
            if (format.Equals("text/html"))//Not suported
                return null;
            if (format.Equals("text/plain"))//Not suported
                return null;

            return null;
        }

        private void OnIdentifyFinished(List<FeatureInfo> featureInfo)
        {
            var handler = IdentifyFinished;
            if (handler != null) handler(this, featureInfo);
        }

        private void OnIdentifyFailed()
        {
            var handler = IdentifyFailed;
            if (handler != null) handler(this, null);
        }
    }
}
