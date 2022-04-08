using System;
using System.IO;
using System.Net;
using BruTile.Extensions;
using Mapsui.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.ArcGIS.DynamicProvider
{
    public delegate void ArcGISLegendEventHandler(object sender, ArcGISLegendResponse? legendInfo);

    /// <summary>
    /// ArcGislegend for getting the layer legends for ArcGIS layers only supports
    /// ArcGISserver 10.0 and up
    /// </summary>
    public class ArcGisLegend
    {
        private int _timeOut;
        private HttpWebRequest? _webRequest;
        private ArcGISLegendResponse? _legendResponse;

        public event ArcGISLegendEventHandler? LegendReceived;
        public event ArcGISLegendEventHandler? LegendFailed;

        public ArcGisLegend()
        {
            TimeOut = 5000;
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 5 seconds
        /// </summary>
        public int TimeOut
        {
            get => _timeOut;
            set => _timeOut = value;
        }

        /// <summary>
        /// Get the legend for the given mapserver
        /// </summary>
        /// <param name="serviceUrl">Url to the mapserver</param>
        /// <param name="credentials">Credentials</param>
        public void GetLegendInfoRequest(string serviceUrl, ICredentials? credentials = null)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        public ArcGISLegendResponse? GetLegendInfo(string serviceUrl, ICredentials? credentials = null)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            using var response = _webRequest.GetSyncResponse(_timeOut);
            _legendResponse = GetLegendResponseFromWebresponse(response);
            return _legendResponse;
        }

        private HttpWebRequest CreateRequest(string serviceUrl, ICredentials? credentials)
        {
            var trailing = serviceUrl.Contains("?") ? "&" : "?";
            var requestUrl = $"{serviceUrl}/legend{trailing}f=json";
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
                using var response = _webRequest.GetSyncResponse(_timeOut);
                _legendResponse = GetLegendResponseFromWebresponse(response);
                using var _ = _webRequest?.EndGetResponse(result);

                if (_legendResponse == null)
                    OnLegendFailed();
                else
                    OnLegendReceived(_legendResponse);
            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                OnLegendFailed();
            }
        }

        private static ArcGISLegendResponse? GetLegendResponseFromWebresponse(WebResponse? webResponse)
        {
            using var dataStream = webResponse?.GetResponseStream();

            if (dataStream != null)
            {
                using var sReader = new StreamReader(dataStream);
                var jsonString = sReader.ReadToEnd();

                var serializer = new JsonSerializer();
                var jToken = JObject.Parse(jsonString);
                using var jTokenReader = new JTokenReader(jToken);
                var legendResponse = serializer.Deserialize(jTokenReader, typeof(ArcGISLegendResponse)) as ArcGISLegendResponse;

                dataStream.Dispose();

                return legendResponse;
            }

            return null;
        }

        private void OnLegendReceived(ArcGISLegendResponse legendInfo)
        {
            var handler = LegendReceived;
            handler?.Invoke(this, legendInfo);
        }

        private void OnLegendFailed()
        {
            var handler = LegendFailed;
            handler?.Invoke(this, null);
        }
    }
}
