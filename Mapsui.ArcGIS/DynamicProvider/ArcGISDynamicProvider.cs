using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mapsui.ArcGIS.Extensions;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Projections;
using Mapsui.Providers;

namespace Mapsui.ArcGIS.DynamicProvider;

public class ArcGISDynamicProvider : IProvider, IProjectingProvider
{
    private int _timeOut;
    private string _url = string.Empty;

    public string? Token { get; set; }
    private string? _crs;
    private readonly IUrlPersistentCache? _persistentCache;

    /// <summary>
    /// Create ArcGisDynamicProvider based on a given capabilities file
    /// </summary>
    /// <param name="url">url to map service example: http://url/arcgis/rest/services/test/MapServer</param>
    /// <param name="arcGisDynamicCapabilities"></param>
    /// <param name="token">token to request service</param>        
    public ArcGISDynamicProvider(string url, ArcGISDynamicCapabilities arcGisDynamicCapabilities, string? token = null, IUrlPersistentCache? persistentCache = null)
    {
        _persistentCache = persistentCache;
        _timeOut = 10000;
        Token = token;

        Url = url;
        ArcGisDynamicCapabilities = arcGisDynamicCapabilities;
    }

    /// <summary>
    /// Create ArcGisDynamicProvider, capabilities will be parsed automatically
    /// </summary>
    /// <param name="url">url to map service example: http://url/arcgis/rest/services/test/MapServer</param>
    /// <param name="token">token to request service</param>
    /// <param name="persistentCache">persistent cache</param>
    public ArcGISDynamicProvider(string url, string? token = null, IUrlPersistentCache? persistentCache = null)
    {
        _persistentCache = persistentCache;
        _timeOut = 10000;
        Token = token;
        Url = url;

        ArcGisDynamicCapabilities = new ArcGISDynamicCapabilities()
        {
            fullExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 },
            initialExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 }
        };

        var capabilitiesHelper = new CapabilitiesHelper(persistentCache);
        capabilitiesHelper.CapabilitiesReceived += CapabilitiesHelperCapabilitiesReceived;
        capabilitiesHelper.CapabilitiesFailed += CapabilitiesHelperCapabilitiesFailed;
        capabilitiesHelper.GetCapabilities(url, CapabilitiesType.DynamicServiceCapabilities, token);
    }

    public ArcGISDynamicCapabilities ArcGisDynamicCapabilities { get; private set; }
    public ICredentials? Credentials { get; set; }

    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            if (!string.IsNullOrEmpty(value) && value[value.Length - 1].Equals('/'))
                _url = value.Remove(value.Length - 1);
        }
    }

    /// <summary>
    /// Timeout of webrequest in milliseconds. Default is 10 seconds
    /// </summary>
    public int TimeOut
    {
        get => _timeOut;
        set => _timeOut = value;
    }

    public string? CRS
    {
        get => _crs;
        set => _crs = value;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        //If there are no layers (probably not initialised) return nothing
        if (ArcGisDynamicCapabilities.layers == null)
            return Enumerable.Empty<IFeature>();

        IViewport viewport = fetchInfo.ToViewport();
        var (success, raster) = await TryGetMapAsync(viewport);
        if (success)
        {
            return new IFeature[] { new RasterFeature(raster) };
        }
        return Enumerable.Empty<IFeature>();
    }

    public MRect? GetExtent()
    {
        if (ArcGisDynamicCapabilities.initialExtent == null)
            return null;

        return ArcGisDynamicCapabilities.initialExtent.ToMRect();
    }

    private void CapabilitiesHelperCapabilitiesFailed(object? sender, EventArgs e)
    {
        Debug.WriteLine("Error getting ArcGIS Capabilities");
    }

    private void CapabilitiesHelperCapabilitiesReceived(object? sender, EventArgs e)
    {
        var capabilities = sender as ArcGISDynamicCapabilities;
        if (capabilities == null)
            return;

        ArcGisDynamicCapabilities = capabilities;
    }

    /// <summary>
    /// Retrieves the bitmap from ArcGIS Dynamic service
    /// </summary>
    public async Task<(bool Success, MRaster? Raster)> TryGetMapAsync(IViewport viewport)
    {
        int width;
        int height;

        try
        {
            width = Convert.ToInt32(viewport.Width);
            height = Convert.ToInt32(viewport.Height);
        }
        catch (OverflowException ex)
        {
            Logger.Log(LogLevel.Error, "Error: Could not convert double to int (ExportMap size)", ex);
            return (false, null);
        }

        try
        {
            var uri = new Uri(GetRequestUrl(viewport.Extent, width, height));
            var bytes = _persistentCache?.Find(uri.ToString());
            if (bytes == null)
            {
                var handler = new HttpClientHandler();
                try
                {
                    // Blazor does not support this,
                    handler.Credentials = Credentials ?? CredentialCache.DefaultCredentials;
                }
                catch (PlatformNotSupportedException e)
                {
                    Logger.Log(LogLevel.Error, e.Message, e);
                };

                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(_timeOut) };

                using var response = await client.GetAsync(uri);
                using var readAsStreamAsync = await response.Content.ReadAsStreamAsync();
                bytes = BruTile.Utilities.ReadFully(readAsStreamAsync);
                _persistentCache?.Add(uri.ToString(), bytes);
            }

            if (viewport.Extent != null)
            {
                var raster = new MRaster(bytes, viewport.Extent);
                return (true, raster);
            }

            return (false, null);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            return (false, null);
        }
    }

    /// <summary>
    /// Gets the URL for a map export request base on current settings, the image size and boundingbox
    /// </summary>
    /// <param name="box">Area the request should cover</param>
    /// <param name="width"> </param>
    /// <param name="height"> </param>
    /// <returns>URL for ArcGIS Dynamic request</returns>
    public string GetRequestUrl(MRect? box, int width, int height)
    {
        //ArcGIS Export description see: http://resources.esri.com/help/9.3/arcgisserver/apis/rest/index.html?export.html

        var sr = CreateSr(CRS);
        var strReq = new StringBuilder(_url);
        strReq.Append("/export?");
        strReq.AppendFormat(CultureInfo.InvariantCulture, "bbox={0},{1},{2},{3}", box?.Min.X, box?.Min.Y, box?.Max.X, box?.Max.Y);
        strReq.AppendFormat("&bboxSR={0}", sr);
        strReq.AppendFormat("&imageSR={0}", sr);
        strReq.AppendFormat("&size={0},{1}", width, height);
        strReq.Append("&layers=show:");

        if (!string.IsNullOrEmpty(Token))
            strReq.Append($"&token={Token}");
        /* 
         * Add all layers to the request that have defaultVisibility to true, the normal request to ArcGIS already does this already
         * without specifying "layers=show", but this adds the opportunity for the user to set the defaultVisibility of layers
         * to false in the capabilities so different views (layers) can be created for one service
         */
        var oneAdded = false;

        if (ArcGisDynamicCapabilities.layers != null)
            foreach (var t in ArcGisDynamicCapabilities.layers)
            {
                if (t.defaultVisibility == false)
                    continue;

                if (oneAdded)
                    strReq.Append(",");

                strReq.AppendFormat("{0}", t.id);
                oneAdded = true;
            }

        strReq.AppendFormat("&format={0}", GetFormat(ArcGisDynamicCapabilities));
        strReq.Append("&transparent=true");
        strReq.Append("&f=image");

        return strReq.ToString();
    }

    private static string CreateSr(string? crs)
    {
        if (crs == null)
            throw new Exception("crs type not supported");

        if (crs.StartsWith(CrsHelper.EsriStringPrefix)) return "{\"wkt\":\"" + crs.Substring(CrsHelper.EsriStringPrefix.Length).Replace("\"", "\\\"") + "\"}";
        if (crs.StartsWith(CrsHelper.EpsgPrefix)) return CrsHelper.ToEpsgCode(crs).ToString();
        throw new Exception("crs type not supported");
    }

    private static string GetFormat(ArcGISDynamicCapabilities arcGisDynamicCapabilities)
    {
        //png | png8 | png24 | jpg | pdf | bmp | gif | svg | png32 (png32 only supported from 9.3.1 and up)
        if (arcGisDynamicCapabilities.supportedImageFormatTypes == null)//Not all services return supported types, use png
            return "png";

        var supportedTypes = arcGisDynamicCapabilities.supportedImageFormatTypes.ToLower();

        if (supportedTypes.Contains("png32"))
            return "png32";
        if (supportedTypes.Contains("png24"))
            return "png24";
        if (supportedTypes.Contains("png8"))
            return "png8";
        if (supportedTypes.Contains("png"))
            return "png";

        return "jpg";
    }

    public bool? IsCrsSupported(string crs)
    {
        return true; // for now assuming ArcGISServer supports all CRSes 
    }
}
