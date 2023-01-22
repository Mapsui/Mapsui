// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Utilities;

namespace Mapsui.Providers.Wms;

/// <summary>
/// Web Map Service layer
/// </summary>
/// <remarks>
/// The WmsLayer is currently very basic and doesn't support automatic fetching of the WMS Service Description.
/// Instead you would have to add the necessary parameters to the URL,
/// and the WmsLayer will set the remaining BoundingBox property and proper requests that changes between the requests.
/// See the example below.
/// </remarks>
public class WmsProvider : IProvider, IProjectingProvider
{
    private string? _mimeType;
    private readonly Client? _wmsClient;
    private Func<string, Task<Stream>>? _getStreamAsync;
    private readonly IUrlPersistentCache? _persistentCache;

    public WmsProvider(XmlDocument capabilities, Func<string, Task<Stream>>? getStreamAsync = null, IUrlPersistentCache? persistentCache = null)
        : this(new Client(capabilities, getStreamAsync), persistentCache: persistentCache)
    {
        InitialiseGetStreamAsyncMethod(getStreamAsync);
    }

    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <param name="url">Url of WMS server</param>
    /// <param name="persistentCache"></param>
    /// <param name="wmsVersion">Version number of wms leave null to get the default service version</param>
    /// <param name="getStreamAsync">Download method, leave null for default</param>
    public static async Task<WmsProvider> CreateAsync(string url, string? wmsVersion = null, Func<string, Task<Stream>>? getStreamAsync = null, IUrlPersistentCache? persistentCache = null)
    {
        var client = await Client.CreateAsync(url, wmsVersion, getStreamAsync, persistentCache: persistentCache);
        var provider = new WmsProvider(client, persistentCache: persistentCache);
        provider.InitialiseGetStreamAsyncMethod(getStreamAsync);
        return provider;
    }

    private WmsProvider(Client wmsClient, Func<string, Task<Stream>>? getStreamAsync = null, IUrlPersistentCache? persistentCache = null)
    {
        _persistentCache = persistentCache;
        InitialiseGetStreamAsyncMethod(getStreamAsync);
        _wmsClient = wmsClient;
        TimeOut = 10000;
        ContinueOnError = true;

        var outputFormats = OutputFormats;
        if (outputFormats.Contains("image/png")) _mimeType = "image/png";
        else if (outputFormats.Contains("image/gif")) _mimeType = "image/gif";
        else if (outputFormats.Contains("image/jpeg")) _mimeType = "image/jpeg";
        else //None of the default formats supported - Look for the first supported output format
        {
            throw new ArgumentException(
                "None of the formats provided by the WMS service are supported");
        }

        LayerList = new Collection<string>();
        StylesList = new Collection<string>();
    }

    private void InitialiseGetStreamAsyncMethod(Func<string, Task<Stream>>? getStreamAsync)
    {
        _getStreamAsync = getStreamAsync ?? GetStreamAsync;
    }

    /// <summary>
    /// Gets the list of enabled layers
    /// </summary>
    public Collection<string>? LayerList { get; private set; }

    /// <summary>
    /// Gets the list of enabled styles
    /// </summary>
    public Collection<string>? StylesList { get; private set; }

    /// <summary>
    /// Gets the hierarchical list of available WMS layers from this service
    /// </summary>
    public Client.WmsServerLayer? RootLayer => _wmsClient?.Layer;

    /// <summary>
    /// Gets the list of available formats
    /// </summary>
    public Collection<string> OutputFormats => _wmsClient?.GetMapOutputFormats ?? new Collection<string>();

    /// <summary>
    /// Gets the list of available FeatureInfo Output Format
    /// </summary>
    public Collection<string> GetFeatureInfoFormats => _wmsClient?.GetFeatureInfoOutputFormats ?? new Collection<string>();

    /// <summary>
    /// Gets the service description from this server
    /// </summary>
    public Capabilities.WmsServiceDescription? ServiceDescription => _wmsClient?.ServiceDescription;

    /// <summary>
    /// Gets the WMS Server version of this service
    /// </summary>
    public string? Version => _wmsClient?.WmsVersion;

    /// <summary>
    /// Specifies whether to throw an exception if the Wms request failed, or to just skip rendering the layer
    /// </summary>
    public bool ContinueOnError { get; set; }

    /// <summary>
    /// Provides the base authentication interface for retrieving credentials for Web client authentication.
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Timeout of web request in milliseconds. Defaults to 10 seconds
    /// </summary>
    public int TimeOut { get; set; }

    /// <summary>
    /// Adds a layer to WMS request
    /// </summary>
    /// <remarks>Layer names are case sensitive.</remarks>
    /// <param name="name">Name of layer</param>
    /// <exception cref="System.ArgumentException">Throws an exception is an unknown layer is added</exception>
    public void AddLayer(string name)
    {
        if (_wmsClient == null || LayerList == null || !LayerExists(_wmsClient.Layer, name))
            throw new ArgumentException("Cannot add WMS Layer - Unknown layer name");

        LayerList.Add(name);
    }

    /// <summary>
    /// Get a layer from the WMS
    /// </summary>
    /// <remarks>Layer names are case sensitive.</remarks>
    /// <param name="name">Name of layer</param>
    /// <exception cref="System.ArgumentException">Throws an exception if the layer is not found</exception>
    public Client.WmsServerLayer GetLayer(string name)
    {
        if (_wmsClient == null)
            throw new InvalidOperationException("WmsClient needs to be set");
        if (FindLayer(_wmsClient.Layer, name, out var layer))
            return layer;

        throw new ArgumentException("Layer not found");
    }

    /// <summary>
    /// Recursive method for checking whether a layer name exists
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool LayerExists(Client.WmsServerLayer layer, string name)
    {
        return name == layer.Name || layer.ChildLayers.Any(childLayer => LayerExists(childLayer, name));
    }

    private bool FindLayer(Client.WmsServerLayer layer, string name, out Client.WmsServerLayer result)
    {
        result = layer;
        if (name == layer.Name)
        {
            return true;
        }

        foreach (var childlayer in layer.ChildLayers)
        {
            if (FindLayer(childlayer, name, out result))
                return true;
        }
        return false;
    }


    /// <summary>
    /// Removes a layer from the layer list
    /// </summary>
    /// <param name="name">Name of layer to remove</param>
    public void RemoveLayer(string name)
    {
        LayerList?.Remove(name);
    }

    /// <summary>
    /// Removes the layer at the specified index
    /// </summary>
    /// <param name="index"></param>
    public void RemoveLayerAt(int index)
    {
        LayerList?.RemoveAt(index);
    }

    /// <summary>
    /// Removes all layers
    /// </summary>
    public void RemoveAllLayers()
    {
        LayerList?.Clear();
    }

    /// <summary>
    /// Adds a style to the style collection
    /// </summary>
    /// <param name="name">Name of style</param>
    /// <exception cref="System.ArgumentException">Throws an exception is an unknown layer is added</exception>
    public void AddStyle(string name)
    {
        if (_wmsClient == null || StylesList == null || !StyleExists(_wmsClient.Layer, name))
            throw new ArgumentException("Cannot add WMS Layer - Unknown layer name");
        StylesList.Add(name);
    }

    /// <summary>
    /// Recursive method for checking whether a layer name exists
    /// </summary>
    /// <param name="layer">layer</param>
    /// <param name="name">name of style</param>
    /// <returns>True of style exists</returns>
    private bool StyleExists(Client.WmsServerLayer layer, string name)
    {
        if (layer.Style.Any(style => name == style.Name)) return true;
        return layer.ChildLayers.Any(childLayer => StyleExists(childLayer, name));
    }

    /// <summary>
    /// Removes a style from the collection
    /// </summary>
    /// <param name="name">Name of style</param>
    public void RemoveStyle(string name)
    {
        StylesList?.Remove(name);
    }

    /// <summary>
    /// Removes a style at specified index
    /// </summary>
    /// <param name="index">Index</param>
    public void RemoveStyleAt(int index)
    {
        StylesList?.RemoveAt(index);
    }

    /// <summary>
    /// Removes all styles from the list
    /// </summary>
    public void RemoveAllStyles()
    {
        StylesList?.Clear();
    }

    /// <summary>
    /// Sets the image type to use when requesting images from the WMS server
    /// </summary>
    /// <remarks>
    /// <para>See the <see cref="OutputFormats"/> property for a list of available mime types supported by the WMS server</para>
    /// </remarks>
    /// <exception cref="ArgumentException">Throws an exception if either the mime type isn't offered by the WMS
    /// or GDI+ doesn't support this mime type.</exception>
    /// <param name="mimeType">Mime type of image format</param>
    public void SetImageFormat(string mimeType)
    {
        if (!OutputFormats.Contains(mimeType))
            throw new ArgumentException("WMS service doesn't not offer mimetype '" + mimeType + "'");
        _mimeType = mimeType;
    }

    public async Task<(bool Success, MRaster?)> TryGetMapAsync(IViewport viewport)
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
            Logger.Log(LogLevel.Error, "Could not convert double to int (ExportMap size)", ex);
            return (false, null);
        }

        var url = GetRequestUrl(viewport.Extent, width, height);

        try
        {
            var bytes = await _persistentCache.UrlCachedArrayAsync(url, _getStreamAsync);

            if (viewport.Extent == null)
            {
                Logger.Log(LogLevel.Warning, "Viewport Extent was null");
                return (false, null);
            }

            var raster = new MRaster(bytes, viewport.Extent);	// This can throw exception
            return (true, raster);
        }
        catch (WebException webEx)
        {
            if (!ContinueOnError)
                throw (new RenderException(
                    "There was a problem connecting to the WMS server",
                    webEx));
            Logger.Log(LogLevel.Error, "There was a problem connecting to the WMS server: " + webEx.Message, webEx);
        }
        catch (Exception ex)
        {
            if (!ContinueOnError)
                throw new RenderException("There was a problem while attempting to request the WMS", ex);
            Logger.Log(LogLevel.Error, "There was a problem while attempting to request the WMS" + ex.Message, ex);
        }

        return (false, null);
    }

    /// <summary>
    /// Gets the URL for a map request base on current settings, the image size and BoundingBox
    /// </summary>
    /// <returns>URL for WMS request</returns>
    public string GetRequestUrl(MRect? box, int width, int height)
    {
        var resource = GetPreferredMethod();
        var strReq = new StringBuilder(resource.OnlineResource);
        if (!resource.OnlineResource?.Contains("?") ?? false)
            strReq.Append("?");
        if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
            strReq.Append("&");
        if (box != null)
            strReq.AppendFormat(CultureInfo.InvariantCulture, "REQUEST=GetMap&BBOX={0},{1},{2},{3}",
                    box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);

        strReq.AppendFormat("&WIDTH={0}&Height={1}", width, height);
        strReq.Append("&Layers=");
        if (LayerList != null && LayerList.Count > 0)
        {
            foreach (var layer in LayerList)
                strReq.AppendFormat("{0},", layer);
            strReq.Remove(strReq.Length - 1, 1);
        }
        strReq.AppendFormat("&FORMAT={0}", _mimeType);
        if (string.IsNullOrWhiteSpace(CRS))
            throw new ApplicationException("Spatial reference system not set");
        if (_wmsClient != null)
        {
            var wmsVersion = _wmsClient.WmsVersion;
            strReq.AppendFormat(wmsVersion != "1.3.0" ? "&SRS={0}" : "&CRS={0}", CRS);
            strReq.AppendFormat("&VERSION={0}", wmsVersion);
        }

        strReq.Append("&TRANSPARENT=true");
        strReq.Append("&Styles=");
        if (StylesList != null && StylesList.Count > 0)
        {
            foreach (var style in StylesList)
                strReq.AppendFormat("{0},", style);
            strReq.Remove(strReq.Length - 1, 1);
        }

        if (ExtraParams != null)
        {
            foreach (var extraParam in ExtraParams)
                strReq.AppendFormat("&{0}={1}", extraParam.Key, extraParam.Value);
        }

        return strReq.ToString();
    }

    /// <summary>
    /// Gets the URL for a map request base on current settings, the image size and BoundingBox
    /// </summary>
    /// <returns>URL for WMS request</returns>
    public IEnumerable<string> GetLegendRequestUrls()
    {
        var legendUrls = new List<string>();
        if (LayerList != null && LayerList.Count > 0)
        {
            foreach (var layer in LayerList)
            {
                if (_wmsClient != null && FindLayer(_wmsClient.Layer, layer, out var result))
                {
                    foreach (var style in result.Style)
                    {
                        var url = WebUtility.HtmlDecode(style.LegendUrl.OnlineResource.OnlineResource);
                        if (url != null)
                        {
                            legendUrls.Add(url);
                            break; // just add first style. TODO: think about how to select a style    
                        }
                    }
                }
            }
        }
        return legendUrls;
    }

    public async IAsyncEnumerable<MemoryStream> GetLegendsAsync()
    {
        var urls = GetLegendRequestUrls();

        foreach (var url in urls)
        {
            if (_getStreamAsync == null)
                yield break;

            using var task = await _getStreamAsync(url);
            var bytes = StreamHelper.ReadFully(task);
            yield return new MemoryStream(bytes);
        }
    }

    private Client.WmsOnlineResource GetPreferredMethod()
    {
        if (_wmsClient == null || _wmsClient.GetMapRequests == null)
            throw new InvalidOperationException("Wms Client needs to be set");
        //We prefer get. Seek for supported 'get' method
        for (var i = 0; i < _wmsClient.GetMapRequests.Length; i++)
            if (_wmsClient.GetMapRequests[i].Type?.ToLower() == "get")
                return _wmsClient.GetMapRequests[i];
        //Next we prefer the 'post' method
        for (var i = 0; i < _wmsClient.GetMapRequests.Length; i++)
            if (_wmsClient.GetMapRequests[i].Type?.ToLower() == "post")
                return _wmsClient.GetMapRequests[i];
        return _wmsClient.GetMapRequests[0];
    }

    public Dictionary<string, string>? ExtraParams { get; set; }

    public string? CRS { get; set; }

    public MRect? GetExtent()
    {
        return CRS != null && _wmsClient != null && _wmsClient.Layer.BoundingBoxes.ContainsKey(CRS) ? _wmsClient.Layer.BoundingBoxes[CRS] : null;
    }

    public bool? IsCrsSupported(string crs)
    {
        if (_wmsClient == null) return null;
        return _wmsClient.Layer.CRS.FirstOrDefault(item => string.Equals(item.Trim(), crs.Trim(), StringComparison.CurrentCultureIgnoreCase)) != null;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var (success, raster) = await TryGetMapAsync(fetchInfo.ToViewport());
        if (success)
            return new[] { new RasterFeature(raster) };
        return Enumerable.Empty<IFeature>();
    }

    private async Task<Stream> GetStreamAsync(string url)
    {
        var handler = new HttpClientHandler { Credentials = Credentials };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
        var req = new HttpRequestMessage(new HttpMethod(GetPreferredMethod().Type ?? "GET"), url);
        var response = await client.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unexpected WMS response code: {response.StatusCode}");
        }

        if (response.Content.Headers.ContentType?.MediaType?.ToLower() != _mimeType)
        {
            throw new Exception($"Unexpected WMS response content type. Expected - {_mimeType}, got - {response.Content.Headers.ContentType?.MediaType}");
        }

        return await response.Content.ReadAsStreamAsync();
    }
}
