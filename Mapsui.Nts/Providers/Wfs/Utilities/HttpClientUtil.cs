// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Providers.Wfs.Utilities;

/// <summary>
/// This class provides an easy to use interface for HTTP-GET and HTTP-POST requests.
/// </summary>
public class HttpClientUtil(IUrlPersistentCache? persistentCache = null)
{
    private readonly Dictionary<string, string?> _requestHeaders = [];
    private byte[]? _postData;
    private string? _proxyUrl;
    private string? _url;
    private ICredentials? _credentials;

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
    /// Adds a HTTP header.
    /// </summary>
    /// <param name="name">The name of the header</param>
    /// <param name="value">The value of the header</param>
    public void AddHeader(string name, string value)
    {
        _requestHeaders.Add(name, value);
    }

    /// <summary>
    /// Performs a HTTP-GET or HTTP-POST request and returns a data stream for reading.
    /// </summary>
    public async Task<Stream?> GetDataStreamAsync()
    {
        if (string.IsNullOrEmpty(_url))
            throw new Exception($"Property {nameof(Url)} was not set");

        var bytes = persistentCache?.Find(_url!, _postData);
        if (bytes != null)
        {
            return new MemoryStream(bytes);
        }

        // Now create a client handler which uses that proxy
        var httpClientHandler = new HttpClientHandler();

        if (!string.IsNullOrEmpty(_proxyUrl))
        {
            var proxy = new WebProxy(_proxyUrl);
            if (Credentials != null)
            {
                proxy.UseDefaultCredentials = false;
                proxy.Credentials = Credentials;
            }

            httpClientHandler.Proxy = proxy;
        }


        if (Credentials != null)
        {
            httpClientHandler.SetUseDefaultCredentials(false);
            httpClientHandler.SetCredentials(Credentials);
        }

        try
        {
            using var httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = new TimeSpan(0, 0, 1, 30);

            foreach (var header in _requestHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            using var webResponse = await GetWebResponseAsync(httpClient).ConfigureAwait(false);

            if (persistentCache != null)
            {
                using var cachedStream = await webResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
                if (cachedStream != null && _url != null)
                {
                    bytes = StreamHelper.ReadFully(cachedStream);
                    persistentCache?.Add(_url, _postData, bytes);
                    return new MemoryStream(bytes);
                }

                return null;
            }

            using var stream = await webResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new MemoryStream(StreamHelper.ReadFully(stream));

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
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occurred during a HTTP request to " + _url + ": " + ex.Message, ex);
            throw;
        }
    }

    private async Task<HttpResponseMessage> GetWebResponseAsync(HttpClient httpClient)
    {
        /* HTTP POST */
        if (_postData != null)
        {
            using var httpContent = new ByteArrayContent(_postData);
            return await httpClient.PostAsync(_url, httpContent);
        }
        /* HTTP GET */
        else
            return await httpClient.GetAsync(_url).ConfigureAwait(false);
    }

    /// <summary>
    /// This method resets all configurations.
    /// </summary>
    public void Reset()
    {
        _url = null;
        _postData = null;
        _requestHeaders.Clear();
    }
}
