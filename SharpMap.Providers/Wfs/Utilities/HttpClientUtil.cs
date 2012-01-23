// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides an easy to use interface for HTTP-GET and HTTP-POST requests.
    /// </summary>
    public class HttpClientUtil
    {
        #region Fields and Properties

        private readonly NameValueCollection _RequestHeaders;
        private byte[] _PostData;
        private string _ProxyUrl;

        private string _Url;
        private HttpWebRequest _WebRequest;
        private HttpWebResponse _WebResponse;

        /// <summary>
        /// Gets ans sets the Url of the request.
        /// </summary>
        public string Url
        {
            get { return _Url; }
            set { _Url = value; }
        }

        /// <summary>
        /// Gets and sets the proxy Url of the request. 
        /// </summary>
        public string ProxyUrl
        {
            get { return _ProxyUrl; }
            set { _ProxyUrl = value; }
        }

        /// <summary>
        /// Sets the data of a HTTP POST request as byte array.
        /// </summary>
        public byte[] PostData
        {
            set { _PostData = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientUtil"/> class.
        /// </summary>
        public HttpClientUtil()
        {
            _RequestHeaders = new NameValueCollection();
        }

        #endregion

        #region Public Member

        /// <summary>
        /// Adds a HTTP header.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="value">The value of the header</param>
        public void AddHeader(string name, string value)
        {
            _RequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Performs a HTTP-GET or HTTP-POST request and returns a datastream for reading.
        /// </summary>
        public Stream GetDataStream()
        {
            if (string.IsNullOrEmpty(_Url))
                throw new ArgumentNullException("Request Url is not set!");

            // Free all resources of the previous request, if it hasn't been done yet...
            Close();

            try
            {
                _WebRequest = (HttpWebRequest) WebRequest.Create(_Url);
            }
            catch (SecurityException ex)
            {
                Trace.TraceError("An exception occured due to security reasons while initializing a request to " + _Url +
                                 ": " + ex.Message);
                throw ex;
            }
            catch (NotSupportedException ex)
            {
                Trace.TraceError("An exception occured while initializing a request to " + _Url + ": " + ex.Message);
                throw ex;
            }

            _WebRequest.Timeout = 90000;

            if (!string.IsNullOrEmpty(_ProxyUrl))
                _WebRequest.Proxy = new WebProxy(_ProxyUrl);

            try
            {
                _WebRequest.Headers.Add(_RequestHeaders);

                /* HTTP POST */
                if (_PostData != null)
                {
                    _WebRequest.ContentLength = _PostData.Length;
                    _WebRequest.Method = WebRequestMethods.Http.Post;
                    using (Stream requestStream = _WebRequest.GetRequestStream())
                    {
                        requestStream.Write(_PostData, 0, _PostData.Length);
                    }
                }
                    /* HTTP GET */
                else
                    _WebRequest.Method = WebRequestMethods.Http.Get;

                _WebResponse = (HttpWebResponse) _WebRequest.GetResponse();
                return _WebResponse.GetResponseStream();
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured during a HTTP request to " + _Url + ": " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// This method resets all configurations.
        /// </summary>
        public void Reset()
        {
            _Url = null;
            _ProxyUrl = null;
            _PostData = null;
            _RequestHeaders.Clear();
        }

        /// <summary>
        /// This method closes the WebResponse object.
        /// </summary>
        public void Close()
        {
            if (_WebResponse != null)
            {
                _WebResponse.Close();
                _WebResponse.GetResponseStream().Dispose();
            }
        }

        #endregion
    }
}