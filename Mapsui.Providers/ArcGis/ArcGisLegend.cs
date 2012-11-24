using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharpMap.Providers.ArcGis
{
    public delegate void ArcgisLegendEventHandler(object sender, ArcGisLegendResponse legendInfo);

    /// <summary>
    /// ArcGislegend for getting the layer legends for ArcGIS layers only supports
    /// ArcGISserver 10.0 and up
    /// </summary>
    public class ArcGisLegend
    {
        private int _timeOut { get; set; }
        private WebRequest _webRequest { get; set; }
        private ArcGisLegendResponse _legendResponse { get; set; }

        public event ArcgisLegendEventHandler LegendReceived;
        public event ArcgisLegendEventHandler LegendFailed;

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
        public void GetLegendInfoAsync(string serviceUrl)
        {
            GetLegendInfoAsync(serviceUrl, CredentialCache.DefaultCredentials);
        }

        public void GetLegendInfoAsync(string serviceUrl, ICredentials credentials)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            _webRequest.BeginGetResponse(FinishWebRequest, null);
        }

        public ArcGisLegendResponse GetLegendInfo(string serviceUrl)
        {
            var legendInfo = GetLegendInfo(serviceUrl, CredentialCache.DefaultCredentials);
            return legendInfo;
        }

        public ArcGisLegendResponse GetLegendInfo(string serviceUrl, ICredentials credentials)
        {
            _webRequest = CreateRequest(serviceUrl, credentials);
            var response = _webRequest.GetResponse();
            _legendResponse = GetLegendResponseFromWebresponse(response);
            return _legendResponse;
        }

        private WebRequest CreateRequest(string serviceUrl, ICredentials credentials)
        {
            var trailing = serviceUrl.Contains("?") ? "&" : "?";
            var requestUrl = string.Format("{0}/legend{1}f=json", serviceUrl, trailing);
            _webRequest = WebRequest.Create(requestUrl);
            _webRequest.Timeout = _timeOut;
            _webRequest.Credentials = credentials;

            return _webRequest;
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                var response = (HttpWebResponse)_webRequest.GetResponse();
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

        private ArcGisLegendResponse GetLegendResponseFromWebresponse(WebResponse webResponse)
        {
            var dataStream = webResponse.GetResponseStream();

            if (dataStream != null)
            {
                var sReader = new StreamReader(dataStream);
                var jsonString = sReader.ReadToEnd();

                var serializer = new JsonSerializer();
                var jToken = JObject.Parse(jsonString);
                var legendResponse = (ArcGisLegendResponse)serializer.Deserialize(new JTokenReader(jToken), typeof(ArcGisLegendResponse));

                dataStream.Close();
                webResponse.Close();

                return legendResponse;
            }

            webResponse.Close();

            return null;
        }

        private void OnLegendReceived(ArcGisLegendResponse legendInfo)
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
