using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BruTile;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.ArcGIS.ImageServiceProvider;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;
using Newtonsoft.Json;

namespace Mapsui.ArcGIS;

public enum CapabilitiesType
{
    ImageServiceCapabilities,
    DynamicServiceCapabilities
}

public class CapabilitiesHelper
{
    private IArcGISCapabilities? _arcGisCapabilities;
    private CapabilitiesType? _capabilitiesType;
    private int _timeOut;
    private string? _url;
    private readonly IUrlPersistentCache? _persistentCache;

    public delegate void StatusEventHandler(object? sender, EventArgs e);

    /// <summary>
    /// Triggered when finished parsing capabilities, returns Capabilities object
    /// </summary>
    public event StatusEventHandler? CapabilitiesReceived;

    /// <summary>
    /// Triggered when failed parsing or getting capabilities
    /// </summary>
    public event StatusEventHandler? CapabilitiesFailed;

    /// <summary>
    /// Helper class for getting capabilities of an ArcGIS service + extras
    /// </summary>
    /// <param name="persistentCache"></param>
    public CapabilitiesHelper(IUrlPersistentCache? persistentCache)
    {
        _persistentCache = persistentCache;
        TimeOut = 10000;
    }

    /// <summary>
    /// Timeout of webrequest in milliseconds. Default is 10 seconds
    /// </summary>
    public int TimeOut
    {
        get => _timeOut;
        set => _timeOut = value;
    }

    /// <summary>
    /// Get the capabilities of an ArcGIS Map Service
    /// </summary>
    /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
    /// <param name="capabilitiesType"></param>
    public void GetCapabilities(string url, CapabilitiesType capabilitiesType)
    {
        ExecuteRequest(url, capabilitiesType);
    }

    /// <summary>
    /// Get the capabilities of an ArcGIS Map Service
    /// </summary>
    /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
    /// <param name="capabilitiesType"></param>
    /// <param name="token">Token string to access the service </param>
    public void GetCapabilities(string url, CapabilitiesType capabilitiesType, string? token)
    {
        ExecuteRequest(url, capabilitiesType, null, token);
    }

    /// <summary>
    /// Get the capabilities of an ArcGIS Map Service
    /// </summary>
    /// <param name="url">Url of map service example: http://url/arcgis/rest/services/test/MapServer </param>
    /// <param name="capabilitiesType"></param>
    /// <param name="credentials">Credentials to access the service </param>
    public void GetCapabilities(string url, CapabilitiesType capabilitiesType, ICredentials credentials)
    {
        ExecuteRequest(url, capabilitiesType, credentials);
    }

    private void ExecuteRequest(string url, CapabilitiesType capabilitiesType, ICredentials? credentials = null, string? token = null)
    {
        Task.Run(async () =>
        {
            _capabilitiesType = capabilitiesType;
            _url = RemoveTrailingSlash(url);

            var requestUri = $"{_url}?f=json";
            if (!string.IsNullOrEmpty(token))
                requestUri = $"{requestUri}&token={token}";

            try
            {
                var data = _persistentCache?.Find(requestUri);
                string? dataStream;

                if (data == null)
                {
                    var handler = new HttpClientHandler();
                    try
                    {
                        // Blazor does not support this,
                        handler.Credentials = credentials ?? CredentialCache.DefaultCredentials;
                    }
                    catch (PlatformNotSupportedException e)
                    {
                        Logger.Log(LogLevel.Error, e.Message, e);
                    };

                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
                    using var response = await client.GetAsync(requestUri);

                    if (!response.IsSuccessStatusCode)
                    {
                        OnCapabilitiesFailed(new EventArgs());
                        return;
                    }

                    dataStream = await response.Content.ReadAsStringAsync();
                    if (_persistentCache != null)
                    {
                        data = Encoding.UTF8.GetBytes(dataStream);
                        _persistentCache.Add(requestUri, data);
                    }
                }
                else
                {
                    dataStream = Encoding.UTF8.GetString(data);
                }

                if (_capabilitiesType == CapabilitiesType.DynamicServiceCapabilities)
                    _arcGisCapabilities = JsonConvert.DeserializeObject<ArcGISDynamicCapabilities>(dataStream);
                else if (_capabilitiesType == CapabilitiesType.ImageServiceCapabilities)
                    _arcGisCapabilities = JsonConvert.DeserializeObject<ArcGISImageCapabilities>(dataStream);

                if (_arcGisCapabilities == null)
                {
                    OnCapabilitiesFailed(EventArgs.Empty);
                    return;
                }

                _arcGisCapabilities.ServiceUrl = _url;

                //Hack because ArcGIS Server doesn't always return a normal StatusCode
                if (dataStream.Contains("{\"error\":{\""))
                {
                    OnCapabilitiesFailed(EventArgs.Empty);
                    return;
                }

                OnFinished(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                OnCapabilitiesFailed(EventArgs.Empty);
            }
        });
    }

    private string RemoveTrailingSlash(string url)
    {
        if (url[url.Length - 1].Equals('/'))
            url = url.Remove(url.Length - 1);

        return url;
    }

    protected virtual void OnFinished(EventArgs e)
    {
        CapabilitiesReceived?.Invoke(_arcGisCapabilities, e);
    }

    protected virtual void OnCapabilitiesFailed(EventArgs e)
    {
        CapabilitiesFailed?.Invoke(null, e);
    }

    /// <summary>
    /// Generate BruTile TileSchema based on ArcGIS Capabilities
    /// </summary>
    /// <returns>TileSchema, returns null if service is not tiled</returns>
    public static ITileSchema? GetTileSchema(ArcGISDynamicCapabilities arcGisDynamicCapabilities)
    {
        //TODO: Does this belong in Mapsui.Providers?

        if (arcGisDynamicCapabilities.tileInfo == null)
            return null;

        var schema = new TileSchema();
        var count = 0;

        if (arcGisDynamicCapabilities.tileInfo.lods != null)
            foreach (var lod in arcGisDynamicCapabilities.tileInfo.lods)
            {
                var level = count;
                schema.Resolutions[level] = new Resolution(level, lod.resolution,
                    arcGisDynamicCapabilities.tileInfo.cols,
                    arcGisDynamicCapabilities.tileInfo.rows);
                count++;
            }

        if (arcGisDynamicCapabilities.fullExtent != null)
            schema.Extent = new BruTile.Extent(arcGisDynamicCapabilities.fullExtent.xmin,
                arcGisDynamicCapabilities.fullExtent.ymin, arcGisDynamicCapabilities.fullExtent.xmax,
                arcGisDynamicCapabilities.fullExtent.ymax);

        if (arcGisDynamicCapabilities.tileInfo.origin != null)
        {
            schema.OriginX = arcGisDynamicCapabilities.tileInfo.origin.x;
            schema.OriginY = arcGisDynamicCapabilities.tileInfo.origin.y;
        }

        schema.Name = "ESRI";
        schema.Format = arcGisDynamicCapabilities.tileInfo.format;
        schema.YAxis = YAxis.OSM;
        if (arcGisDynamicCapabilities.tileInfo.spatialReference != null)
            schema.Srs = $"EPSG:{arcGisDynamicCapabilities.tileInfo.spatialReference.wkid}";

        return schema;
    }
}
