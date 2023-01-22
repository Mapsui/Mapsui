using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BruTile.Extensions;
using Mapsui.Cache;
using Mapsui.Logging;
using Mapsui.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.ArcGIS.DynamicProvider;

public delegate void ArcGISLegendEventHandler(object sender, ArcGISLegendResponse? legendInfo);

/// <summary>
/// ArcGislegend for getting the layer legends for ArcGIS layers only supports
/// ArcGISserver 10.0 and up
/// </summary>
public class ArcGisLegend
{
    private int _timeOut;
    private ArcGISLegendResponse? _legendResponse;
    private readonly IUrlPersistentCache? _urlPersistentCache;

    public event ArcGISLegendEventHandler? LegendReceived;
    public event ArcGISLegendEventHandler? LegendFailed;

    public ArcGisLegend(IUrlPersistentCache? urlPersistentCache = null)
    {
        _urlPersistentCache = urlPersistentCache;
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
#pragma warning disable VSTHRD110 // observe the awaitable
        Task.Run(async () =>
#pragma warning restore VSTHRD110
        {
            try
            {
                var result = await GetLegendInfoAsync(serviceUrl, credentials);
                if (result != null)
                    OnLegendReceived(result);
                else
                    OnLegendFailed();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
                OnLegendFailed();
            }
        });
    }

    public async Task<ArcGISLegendResponse?> GetLegendInfoAsync(string serviceUrl, ICredentials? credentials = null)
    {
        var uri = CreateRequestUrl(serviceUrl);
        var data = _urlPersistentCache?.Find(uri);
        Stream stream;
        if (data == null)
        {
            using var httpClient = CreateRequest(credentials);
            using var response = await httpClient.GetAsync(uri);
            stream = await response.Content.ReadAsStreamAsync();
            data = StreamHelper.ReadFully(stream);
            _urlPersistentCache?.Add(uri, data);
#if NET6_0_OR_GREATER                    
            await stream.DisposeAsync();
#else
            stream.Dispose();
#endif
        }

        stream = new MemoryStream(data);
        _legendResponse = GetLegendResponseFromWebResponse(stream);
        return _legendResponse;
    }

    private HttpClient CreateRequest(ICredentials? credentials)
    {
        HttpClientHandler httpClientHandler = new HttpClientHandler();
        try
        {
            // Blazor does not support this.
            httpClientHandler.UseDefaultCredentials = credentials == null;
        }
        catch (PlatformNotSupportedException e)
        {
            Logger.Log(LogLevel.Error, e.Message, e);
        }

        if (credentials != null) httpClientHandler.Credentials = credentials;

        var httpClient = new HttpClient(httpClientHandler)
        {
            Timeout = TimeSpan.FromMilliseconds(_timeOut),
        };
        return httpClient;
    }

    private static string CreateRequestUrl(string serviceUrl)
    {
        var trailing = serviceUrl.Contains("?") ? "&" : "?";
        var requestUrl = $"{serviceUrl}/legend{trailing}f=json";
        return requestUrl;
    }

    private static ArcGISLegendResponse? GetLegendResponseFromWebResponse(Stream? dataStream)
    {
        if (dataStream != null)
        {
            using var sReader = new StreamReader(dataStream);
            var jsonString = sReader.ReadToEnd();

            var serializer = new JsonSerializer();
            var jToken = JObject.Parse(jsonString);
            using var jTokenReader = new JTokenReader(jToken);
            var legendResponse = serializer.Deserialize(jTokenReader, typeof(ArcGISLegendResponse)) as ArcGISLegendResponse;

#pragma warning disable IDISP007 // don't dispose injected
            dataStream.Dispose();
#pragma warning restore IDISP007                

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
