using BruTile.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace Mapsui.ArcGISDynamicLayer
{
    public delegate void ArcGISLegendEventHandler(object sender, ArcGISLegendResponse legendInfo);

    /// <summary>
    /// ArcGislegend for getting the layer legends for ArcGIS layers only supports
    /// ArcGISserver 10.0 and up
    /// </summary>
    public class ArcGisLegend
    {
        private int _timeOut;
        private HttpWebRequest _webRequest;
        private ArcGISLegendResponse _legendResponse;

        public event ArcGISLegendEventHandler LegendReceived;
        public event ArcGISLegendEventHandler LegendFailed;

        public ArcGisLegend()
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
        /// Get the legend for the given mapserver
        /// </summary>
        /// <param name="serviceUrl">Url to the mapserver</param>
        /// <param name="credentials">Credentials</param>
        public void GetLegendInfoAsync(string serviceUrl, ICredentials credentials = null)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        public ArcGISLegendResponse GetLegendInfo(string serviceUrl, ICredentials credentials = null)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            var response = _webRequest.GetSyncResponse(_timeOut);
            _legendResponse = GetLegendResponseFromWebresponse(response);
            return _legendResponse;
        }

        private HttpWebRequest CreateRequest(string serviceUrl, ICredentials credentials)
        {
            var trailing = serviceUrl.Contains("?") ? "&" : "?";
            var requestUrl = string.Format("{0}/legend{1}f=json", serviceUrl, trailing);
            _webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            if (credentials == null)
                _webRequest.UseDefaultCredentials = true;
            else
                _webRequest.Credentials = credentials;

            return _webRequest;
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = _webRequest.GetSyncResponse(_timeOut);
                _legendResponse = GetLegendResponseFromWebresponse(response);
                _webRequest.EndGetResponse(result);

                if (_legendResponse == null)
                    OnLegendFailed();
                else
                    OnLegendReceived(_legendResponse);
            }
            catch (WebException)
            {
                OnLegendFailed();
            }
        }

        private ArcGISLegendResponse GetLegendResponseFromWebresponse(WebResponse webResponse)
        {
            var dataStream = webResponse.GetResponseStream();

            if (dataStream != null)
            {
                var sReader = new StreamReader(dataStream);
                var jsonString = sReader.ReadToEnd();

                var serializer = new JsonSerializer();
                var jToken = JObject.Parse(jsonString);
                var legendResponse = (ArcGISLegendResponse)serializer.Deserialize(new JTokenReader(jToken), typeof(ArcGISLegendResponse));

                dataStream.Dispose();
                webResponse.Dispose();

                return legendResponse;
            }

            webResponse.Dispose();

            return null;
        }

        private void OnLegendReceived(ArcGISLegendResponse legendInfo)
        {
            var handler = LegendReceived;
            if (handler != null) handler(this, legendInfo);
        }

        private void OnLegendFailed()
        {
            var handler = LegendFailed;
            if (handler != null) handler(this, null);
        }
    }
}
