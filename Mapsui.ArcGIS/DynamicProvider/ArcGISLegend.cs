using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Utilities;

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

    public Task<ArcGISLegendResponse?> GetLegendInfoAsync(string serviceUrl, ICredentials? credentials = null)
    {
        return GetLegendInfoAsync(serviceUrl, CancellationToken.None, credentials);
    }

    public async Task<ArcGISLegendResponse?> GetLegendInfoAsync(string serviceUrl, CancellationToken cancellationToken, ICredentials? credentials = null)
    {
        var uri = CreateRequestUrl(serviceUrl);
        var data = _urlPersistentCache?.Find(uri);
        if (data == null)
        {
            using var httpClient = CreateRequest(credentials);
            using var response = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            data = StreamHelper.ReadFully(stream);
            _urlPersistentCache?.Add(uri, data);
            await stream.DisposeAsync();
        }

        _legendResponse = GetLegendResponseFromWebResponse(data);
        return _legendResponse;
    }

    private HttpClient CreateRequest(ICredentials? credentials)
    {
        HttpClientHandler httpClientHandler = new HttpClientHandler();
        httpClientHandler.SetUseDefaultCredentials(credentials == null);

        if (credentials != null)
        {
            httpClientHandler.SetCredentials(credentials);
        }

        var httpClient = new HttpClient(httpClientHandler)
        {
            Timeout = TimeSpan.FromMilliseconds(_timeOut),
        };
        return httpClient;
    }

    private static string CreateRequestUrl(string serviceUrl)
    {
        var trailing = serviceUrl.Contains('?') ? "&" : "?";
        var requestUrl = $"{serviceUrl}/legend{trailing}f=json";
        return requestUrl;
    }

    private static ArcGISLegendResponse? GetLegendResponseFromWebResponse(byte[]? data)
    {
        if (data == null)
        {
            return null;
        }
        using var stream = new MemoryStream(data);

        var legendResponse = JsonSerializer.Deserialize(stream, ArcGISContext.Default.ArcGISLegendResponse);
        return legendResponse;
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
