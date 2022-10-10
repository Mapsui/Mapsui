// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using Mapsui.Cache;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Providers.Wfs.Utilities
{
    /// <summary>
    /// This class provides an easy to use interface for HTTP-GET and HTTP-POST requests.
    /// </summary>
    public class HttpClientUtil : IDisposable
    {

        private readonly NameValueCollection _requestHeaders;
        private byte[]? _postData;
        private string? _proxyUrl;
        private string? _url;
        private HttpWebRequest? _webRequest;
        private HttpWebResponse? _webResponse;
        private ICredentials? _credentials;
        private readonly IUrlPersistentCache? _persistentCache;

        /// <summary>
        /// Gets ans sets the Url of the request.
        /// </summary>
        public string? Url
        {
            get => _url;
            set => _url = value;
        }

        /// <summary>
        /// Gets and sets the proxy Url of the request. 
        /// </summary>
        public string? ProxyUrl
        {
            get => _proxyUrl;
            set => _proxyUrl = value;
        }

        /// <summary>
        /// Sets the data of a HTTP POST request as byte array.
        /// </summary>
        public byte[] PostData
        {
            set => _postData = value;
        }

        /// <summary>
        /// Gets or sets the network credentials used for authenticating the request with the Internet resource
        /// </summary>
        public ICredentials? Credentials
        {
            get => _credentials;
            set => _credentials = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientUtil"/> class.
        /// </summary>
        public HttpClientUtil(IUrlPersistentCache? persistentCache = null)
        {
            _persistentCache = persistentCache;
            _requestHeaders = new NameValueCollection();
        }



        /// <summary>
        /// Adds a HTTP header.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="value">The value of the header</param>
        public void AddHeader(string name, string value)
        {
            _requestHeaders.Add(name, value);
        }

        /// <summary>
        /// Performs a HTTP-GET or HTTP-POST request and returns a datastream for reading.
        /// </summary>
        public Stream? GetDataStream()
        {
            if (string.IsNullOrEmpty(_url))
                throw new Exception($"Property {nameof(Url)} was not set");

            var bytes = _persistentCache?.Find(_url!);
            if (bytes != null)
            {
                return new MemoryStream(bytes);
            }

            // Free all resources of the previous request, if it hasn't been done yet...
            Close();

            try
            {
                _webRequest = (HttpWebRequest)WebRequest.Create(_url);
            }
            catch (SecurityException ex)
            {
                Logger.Log(LogLevel.Error, "An exception occurred due to security reasons while initializing a request to " + _url +
                                           ": " + ex.Message, ex);
                throw;
            }
            catch (NotSupportedException ex)
            {
                Logger.Log(LogLevel.Error, "An exception occurred while initializing a request to " + _url + ": " + ex.Message, ex);
                throw;
            }

            _webRequest.Timeout = 90000;

            if (!string.IsNullOrEmpty(_proxyUrl))
                _webRequest.Proxy = new WebProxy(_proxyUrl);

            try
            {
                _webRequest.Headers.Add(_requestHeaders);

                if (Credentials != null)
                {
                    _webRequest.UseDefaultCredentials = false;
                    _webRequest.Credentials = Credentials;
                }

                /* HTTP POST */
                if (_postData != null)
                {
                    _webRequest.ContentLength = _postData.Length;
                    _webRequest.Method = WebRequestMethods.Http.Post;
                    using (var requestStream = _webRequest.GetRequestStream())
                    {
                        requestStream.Write(_postData, 0, _postData.Length);
                    }
                }
                /* HTTP GET */
                else
                    _webRequest.Method = WebRequestMethods.Http.Get;

                _webResponse?.Dispose();
                _webResponse = (HttpWebResponse)_webRequest.GetResponse();
                if (_persistentCache != null)
                {
                    using var stream = _webResponse.GetResponseStream();
                    if (stream != null && _url != null)
                    {
                        bytes = StreamHelper.ReadFully(stream);
                        _persistentCache?.Add(_url, bytes);
                        return new MemoryStream(bytes);    
                    }

                    return null;
                }

                return _webResponse.GetResponseStream();

            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "An exception occurred during a HTTP request to " + _url + ": " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// This method resets all configurations.
        /// </summary>
        public void Reset()
        {
            _url = null;
            _proxyUrl = null;
            _postData = null;
            _requestHeaders.Clear();
        }

        /// <summary>
        /// This method closes the WebResponse object.
        /// </summary>
        public void Close() //This class should implement dispose instead.
        {
            if (_webResponse != null)
            {
                // ATTENTION: Dispose first the Response Stream
                // or else a disposed exception occurs.
                var responseStream = _webResponse?.GetResponseStream();
                responseStream?.Dispose();
                _webResponse?.Close();
                _webResponse?.Dispose();
                _webResponse = null;
            }
        }

        public virtual void Dispose()
        {
            _webResponse?.Dispose();
        }
    }
}